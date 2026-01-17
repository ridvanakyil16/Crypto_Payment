using Microsoft.AspNetCore.Identity;

namespace Crypto_Payment.Models;

public class Role : IdentityRole
{
    public string Name { get; set; }    
    public string NormalizedName { get; set; }    
    public string ConcurrencyStamp { get; set; }    
}