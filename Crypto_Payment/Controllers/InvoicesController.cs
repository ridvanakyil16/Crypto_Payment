using Crypto_Payment.DTOS;
using Crypto_Payment.Services;
using Microsoft.AspNetCore.Mvc;

namespace Crypto_Payment.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;
    private readonly IPlisioService _plisioService;

    public InvoicesController(IInvoiceService service, IPlisioService plisioService)
    {
        _service = service;
        _plisioService = plisioService;
    }
    
    [HttpPost("invoice-add")]
    public async Task<IActionResult> InvoiceAdd([FromBody] InvoiceDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // iş kuralı hatası (Plisio, ödeme reddi vs.)
            return UnprocessableEntity(new
            {
                message = ex.Message
            });
        }
        catch (Exception)
        {
            // beklenmeyen sistem hatası
            return StatusCode(500, new
            {
                message = "Beklenmeyen bir hata oluştu."
            });
        }
    }
    
    // LIST
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { title = "Sunucu hatası", detail = ex.Message });
        }
    }
    
    // STATUS CHECK - Ödeme durumunu kontrol et
    [HttpGet("status/{id}")]
    public async Task<IActionResult> GetStatus(int id)
    {
        try
        {
            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(new { status = "not_found" });
            }
            
            var currentStatus = invoice.Status ?? "pending";
            
            // Plisio'dan her zaman güncel durumu kontrol et (completed/expired bile olsa)
            if (!string.IsNullOrEmpty(invoice.TxnId))
            {
                var plisioDetails = await _plisioService.GetInvoiceDetailsAsync(invoice.TxnId);
                if (plisioDetails != null && !string.IsNullOrEmpty(plisioDetails.Status))
                {
                    var plisioStatus = plisioDetails.Status;
                    var newStatus = MapPlisioStatus(plisioStatus);
                    
                    Console.WriteLine($"[STATUS CHECK] Invoice {id}: DB={currentStatus}, Plisio={plisioStatus}, Mapped={newStatus}");
                    
                    // Durum değiştiyse güncelle
                    if (newStatus != currentStatus)
                    {
                        await _service.UpdateStatusAsync(id, newStatus);
                        Console.WriteLine($"[STATUS UPDATE] Invoice {id}: {currentStatus} → {newStatus}");
                        return Ok(new { status = newStatus, updated = true, oldStatus = currentStatus });
                    }
                }
                else
                {
                    Console.WriteLine($"[STATUS CHECK] Invoice {id}: Plisio details null or status empty");
                }
            }
            
            return Ok(new { status = currentStatus, updated = false });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[STATUS ERROR] Invoice {id}: {ex.Message}");
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }
    
    private string MapPlisioStatus(string plisioStatus)
    {
        return plisioStatus?.ToLower() switch
        {
            "new" => "new",
            "completed" => "completed",
            "mismatch" => "mismatch", // Tutar farklı ama ödeme yapılmış
            "expired" => "expired",
            "cancelled" => "cancelled",
            "error" => "error",
            _ => "pending"
        };
    }
}
