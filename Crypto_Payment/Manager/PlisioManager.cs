using System.Globalization;
using System.Text.Json;
using Crypto_Payment.Services;
using Microsoft.Extensions.Options;

namespace Crypto_Payment.Manager;

public class PlisioManager : IPlisioService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public PlisioManager(HttpClient http, IOptions<PlisioOptions> opts)
    {
        _http = http;
        _apiKey = opts.Value.ApiKey;
        _baseUrl = opts.Value.BaseUrl;
    }

    public async Task<PlisioInvoiceResult> CreateInvoiceAsync(CreateInvoiceDto dto, string callbackUrl)
    {
        var url =
            "https://api.plisio.net/api/v1/invoices/new" +
            $"?source_currency={Uri.EscapeDataString(dto.SourceCurrency)}" +
            $"&source_amount={Uri.EscapeDataString(dto.SourceAmount.ToString(CultureInfo.InvariantCulture))}" +
            $"&order_number={Uri.EscapeDataString(dto.OrderNumber)}" +
            $"&currency={Uri.EscapeDataString(dto.Currency)}" +
            $"&email={Uri.EscapeDataString(dto.Email)}" +
            $"&order_name={Uri.EscapeDataString(dto.OrderName)}" +
            $"&callback_url={Uri.EscapeDataString(callbackUrl)}" +
            $"&api_key={Uri.EscapeDataString(_apiKey)}";
        var resp = await _http.GetAsync(url);
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();
        if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
        {
            var data = doc.RootElement.GetProperty("data");
            var txnId = data.GetProperty("txn_id").GetString();
            var invoiceUrl = data.GetProperty("invoice_url").GetString();
            
            return new PlisioInvoiceResult
            {
                IsSuccess = true,
                InvoiceId = txnId,
                TxnId = txnId,
                InvoiceUrl = invoiceUrl
            };
        }
        else
        {
            var data = doc.RootElement.GetProperty("data");
            return new PlisioInvoiceResult
            {
                IsSuccess = false,
                ErrorMessage = data.TryGetProperty("message", out var m) ? m.GetString() : body,
                ErrorCode = data.TryGetProperty("code", out var c) ? c.GetInt32() : (int?)null
            };
        }
    }
    
    public async Task<PlisioInvoiceDetails?> GetInvoiceDetailsAsync(string? txnId)
    {
        if (string.IsNullOrEmpty(txnId)) return null;
        try
        {
            // Use /invoices endpoint to get full details including wallet_hash
            var url = $"https://api.plisio.net/api/v1/invoices/{txnId}?api_key={Uri.EscapeDataString(_apiKey)}";
            var resp = await _http.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var status = doc.RootElement.GetProperty("status").GetString();
            if (!string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            
            var data = doc.RootElement.GetProperty("data");
            
            // Invoice details are nested inside data.invoice
            if (!data.TryGetProperty("invoice", out var invoice))
            {
                return null;
            }
            
            var details = new PlisioInvoiceDetails
            {
                Status = invoice.TryGetProperty("status", out var s) ? s.GetString() : null,
                Amount = invoice.TryGetProperty("amount", out var a) ? a.GetString() : null,
                Currency = invoice.TryGetProperty("currency", out var cur) ? cur.GetString() : null,
                TxIds = new List<string>()
            };
            
            // Wallet address
            if (invoice.TryGetProperty("wallet_hash", out var wh))
            {
                details.WalletAddress = wh.GetString();
            }
            
            // Expire time (Unix timestamp as string)
            if (invoice.TryGetProperty("expire_utc", out var exp))
            {
                string? expStr = null;
                if (exp.ValueKind == JsonValueKind.String)
                {
                    expStr = exp.GetString();
                }
                else if (exp.ValueKind == JsonValueKind.Number)
                {
                    expStr = exp.GetInt64().ToString();
                }
                
                if (!string.IsNullOrEmpty(expStr) && long.TryParse(expStr, out var expUnix))
                {
                    details.ExpireTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                }
            }
            
            // QR Code URL - Generate from wallet address
            if (invoice.TryGetProperty("wallet_hash", out var walletForQr))
            {
                var walletAddr = walletForQr.GetString();
                if (!string.IsNullOrEmpty(walletAddr))
                {
                    details.QrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={Uri.EscapeDataString(walletAddr)}";
                }
            }
            
            // Transaction IDs from tx array
            if (invoice.TryGetProperty("tx", out var txArray) && txArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var tx in txArray.EnumerateArray())
                {
                    if (tx.TryGetProperty("txid", out var txId))
                    {
                        var txIdStr = txId.GetString();
                        if (!string.IsNullOrEmpty(txIdStr))
                        {
                            details.TxIds.Add(txIdStr);
                        }
                    }
                }
            }
            
            // Also check tx_id field
            if (invoice.TryGetProperty("tx_id", out var singleTxId))
            {
                var txIdStr = singleTxId.GetString();
                if (!string.IsNullOrEmpty(txIdStr) && !details.TxIds.Contains(txIdStr))
                {
                    details.TxIds.Add(txIdStr);
                }
            }
            
            return details;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    private static string AddJsonTrue(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        if (url.Contains("json=true", StringComparison.OrdinalIgnoreCase)) return url;
        return url.Contains("?") ? url + "&json=true" : url + "?json=true";
    }
}

public class PlisioInvoiceResult
{
    public bool IsSuccess { get; set; }
    public string? InvoiceId { get; set; }
    public string? TxnId { get; set; }
    public string? InvoiceUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public int? ErrorCode { get; set; }
}

public class PlisioOptions
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.plisio.net";
}
