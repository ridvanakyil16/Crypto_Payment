namespace Crypto_Payment.Models;

public class Invoice
{
    public int Id { get; set; }
    public string SourceCurrency { get; set; }   // USD
    public decimal SourceAmount { get; set; }    // 2
    public string OrderNumber { get; set; }      // 1
    public string Currency { get; set; }         // UDST_TRX
    public string Email { get; set; }
    public string OrderName { get; set; }        // usdt
    public string CallbackUrl { get; set; }      // http://...
}