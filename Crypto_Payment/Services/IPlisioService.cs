using Crypto_Payment.DTOS;

namespace Crypto_Payment.Services;

public interface IPlisioService
{
    public Task<PlisioInvoiceResult> CreateInvoiceAsync(InvoiceDto dto);
}