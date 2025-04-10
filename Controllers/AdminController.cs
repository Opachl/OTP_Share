using Microsoft.AspNetCore.Mvc;
using OTP_Share.Models;
using OTP_Share.Services;
using OTP_Share.Services.StateDBService;
using OTP_Share.Services.VaultwardenService;

namespace OTP_Share.Controllers
{
  public class AdminController : Controller
  {
    private readonly IVaultService _vaultService;
    private readonly DatalayerService _DLService;
    private readonly IStateDBService _stateDBService;

    public AdminController(IVaultService vaultService, IStateDBService stateDBService, DatalayerService datalayerService)
    {
      _vaultService = vaultService;
      _stateDBService = stateDBService;
      _DLService = datalayerService;
    }

    public IActionResult Index()
    {
      TempData["GroupID"] = "asdasd";

      if(HttpContext.Session.GetString("IsAdmin") == "true")
      {
        var validItems = _stateDBService.GetValidEntrys();
        var items = _vaultService.GetItems().Select(x => new ItemViewModel
        {
          Id = x.id,
          Title = x.name,
          HasTOPT = x?.login?.totp != null,
          ActiveGroupIDs = validItems.Where(y => y.VaultItemId == x.id).Select(y => y.ShareGroupID).ToList()
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