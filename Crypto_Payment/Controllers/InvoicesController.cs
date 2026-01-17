using Crypto_Payment.DTOS;
using Crypto_Payment.Services;
using Microsoft.AspNetCore.Mvc;

namespace Crypto_Payment.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;

    public InvoicesController(IInvoiceService service)
    {
        _service = service;
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
}