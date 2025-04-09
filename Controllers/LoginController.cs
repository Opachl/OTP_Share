using Microsoft.AspNetCore.Mvc;
using OTP_Share.Services.EnvironmentService;

namespace OTP_Share.Controllers
{
  public class LoginController : Controller
  {
    private readonly IEnvironmentService _EnvSrv;

    public LoginController(IEnvironmentService environment)
    {
      _EnvSrv = environment;
    }

    public IActionResult Index()
    {
      return View();
    }

    [HttpPost]
    public IActionResult Authenticate(string username, string password)
    {
      if(_EnvSrv.AdminPageEnabled && username == _EnvSrv.AdminUser && password == _EnvSrv.AdminPassword)
      {
        HttpContext.Session.SetString("IsAdmin", "true");
        return RedirectToAction("Index", "Admin");
      }

      ViewBag.Error = "Invalid credentials";
      return View("Index");
    }

    public IActionResult Logout()
    {
      HttpContext.Session.Remove("IsAdmin");
      return RedirectToAction("Index");
    }
  }
}