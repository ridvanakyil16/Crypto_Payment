using Crypto_Payment.DTOS;
using Crypto_Payment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Crypto_Payment.Controllers;

[Authorize]
[Route("users")]
public class UserController : Controller
{
    // GET
    [HttpGet]
    public IActionResult UserList()
    {
        return View();
    }
}