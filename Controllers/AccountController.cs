using Microsoft.AspNetCore.Mvc;
using OTP_Share.Models;
using OTP_Share.Services;
using OTP_Share.Services.StateDBService;
using OTP_Share.Services.VaultwardenService;

namespace OTP_Share.Controllers
{
  public class AccountController : Controller
  {
    private readonly IVaultService _vaultService;
    private readonly DatalayerService _DLService;
    private readonly IStateDBService _stateService;

    public AccountController(IStateDBService stateService, IVaultService vaultService, DatalayerService datalayerService)
    {
      _stateService = stateService;
      _vaultService = vaultService;
      _DLService = datalayerService;
    }

    public IActionResult Index(string group)
    {
      EnsureViewBackAccResponse();

      IEnumerable<GroupItemViewModel> entrys = null;

      var isGUID = Guid.TryParse(group, out var guid);
      if(!string.IsNullOrEmpty(group) && isGUID)
      {
        entrys = _stateService.GetValidEntrys(guid).Select(x => new GroupItemViewModel(x.ShareGroupID, x.Id, x.VaultItemId)
        {
          DisplayName = x.DisplayName,
          ExpireDate = x.ExpireDate,

          MaxTOPTRequests = x.MaxTOPTRequests,
          RequestedTOPTRequests = x.RequestedTOPTRequests,
        });
      }

      if(entrys == null || !entrys.Any())
      {
        TempData["ErrorMessage"] = "Valid Group ID is required.";
        TempData["RedirectUrl"] = Url.Action("Index", "Home");
        return View("RedirectWithMessage");
      }

      var ctx = HttpContext.Session.GetString("ctx");
      if(!string.IsNullOrEmpty(ctx))
      {
        var decodedBytes = Convert.FromBase64String(ctx);
        var originalResponse = System.Text.Json.JsonSerializer.Deserialize<ShareItemResponse>(decodedBytes);
        HttpContext.Session.Remove("ctx");

        EnsureViewBackAccResponse(originalResponse, group);
      }

      return View(entrys);
    }

    [HttpPost]
    public IActionResult Request(string itemId)
    {
      var groupID = GroupItemViewModel.GetGroupID(itemId);
      var stateID = GroupItemViewModel.GetStateDBId(itemId);
      var vaultID = GroupItemViewModel.GetVaultItemId(itemId);

      var response = _DLService.IncreaseShareUsage(new ShareItem()
      {
        GroupID = groupID,
        DBID = stateID,
        VaultID = vaultID
      });

      var responseCryptedString = Convert.ToBase64String(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(response));
      HttpContext.Session.SetString("ctx", responseCryptedString);

      return RedirectToAction("Index", new { group = groupID });
    }

    [HttpPost]
    public IActionResult AcknowledgeResponse(string groupID)
    {
      // Clear session (optional, to prevent re-use)
      HttpContext.Session.Remove("ctx");

      // Update ViewBag (not strictly needed if redirecting, but safe)
      EnsureViewBackAccResponse();

      return RedirectToAction("Index", new { group = groupID });
    }

    private void EnsureViewBackAccResponse(ShareItemResponse originalResponse = null, string groupID = null)
    {
      ViewBag.ShowAccResponse = false;
      ViewBag.GID = null;
      ViewBag.AccResponse = null;

      if(originalResponse != null && !string.IsNullOrEmpty(groupID) && originalResponse.Success)
      {
        ViewBag.ShowAccResponse = true;
        ViewBag.GID = groupID;
        ViewBag.AccResponse = originalResponse;
      }
    }
  }
}