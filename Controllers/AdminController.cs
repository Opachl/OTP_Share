using Microsoft.AspNetCore.Mvc;
using OTP_Share.Models;
using OTP_Share.Services;
using OTP_Share.Services.VaultwardenService;

namespace OTP_Share.Controllers
{
  public class AdminController : Controller
  {
    private readonly IVaultService _vaultService;
    private readonly DatalayerService _DLService;

    public AdminController(IVaultService vaultService, DatalayerService datalayerService)
    {
      _vaultService = vaultService;
      _DLService = datalayerService;
    }

    public IActionResult Index()
    {
      TempData["GroupID"] = "asdasd";

      if(HttpContext.Session.GetString("IsAdmin") == "true")
      {
        var items = _vaultService.GetItems().Select(x => new ItemViewModel
        {
          Id = x.id,
          Title = x.name,
          HasTOPT = x?.login?.totp != null
        }).ToList();

        return View(items);
      }
      else
      {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
      }
    }

    [HttpPost]
    public IActionResult Register(RegisterItemViewModel model)
    {
      var result = _DLService.RegisterNewShare(model.ItemId, model.MaxUsages, model.ExpireDate);

      TempData["GroupID"] = result.GroupID;

      // Redirect or return success
      return RedirectToAction("Index");
    }
  }
}