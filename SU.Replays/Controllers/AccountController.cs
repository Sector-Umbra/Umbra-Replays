using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace SU.Replays.Controllers;

public class AccountController : Controller
{
    [HttpGet("logout")]
    public IActionResult Logout()
    {
        return SignOut(new AuthenticationProperties() { RedirectUri = "/signed-out" });
    }
}