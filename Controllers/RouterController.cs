using Microsoft.AspNetCore.Mvc;

namespace OTP_Share.Controllers
{
  public class RouterController : Controller
  {
    public IActionResult Index(string? group)
    {
      if(!string.IsNullOrEmpty(group))
      {
        // Internally redirect to AccountController
        return RedirectToAction("Index", "Account", new { group });
      }

      // Internally redirect to HomeController
      return RedirectToAction("Index", "Home");
    }
  }
}