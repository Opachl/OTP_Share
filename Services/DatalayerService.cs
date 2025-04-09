using OTP_Share.Services.NTPService;
using OTP_Share.Services.StateDBService;
using OTP_Share.Services.VaultwardenService;

namespace OTP_Share.Services
{
  public class DatalayerService
  {
    private readonly IStateDBService _stateDBService;
    private readonly IVaultService _vaultService;
    private readonly INTPService _nTPService;

    public DatalayerService(IStateDBService stateDB, IVaultService vaultService, INTPService nTPService)
    {
      _stateDBService = stateDB;
      _vaultService = vaultService;
      _nTPService = nTPService;
    }

    public ShareItem RegisterNewShare(string vaultID, int maxShares, DateTime expireDate)
    {
      ShareItem result = null;

      var vaultElement = _vaultService.GetItems().Where(x => x.id == vaultID).FirstOrDefault();
      if(vaultElement != null)
      {
        var dbEntry = new StateDBService.Model.DBSharedLink()
        {
          VaultItemId = vaultElement.id,
          CreatedDate = DateTime.Now,
          DisplayName = vaultElement.name,
          ExpireDate = expireDate,
          MaxTOPTRequests = maxShares,
          ShareGroupID = Guid.NewGuid()
        };

        _stateDBService.AddOrUpdateEntry(dbEntry);

        result = new ShareItem()
        {
          DBID = dbEntry.Id,
          VaultID = dbEntry.VaultItemId,
          GroupID = dbEntry.ShareGroupID.ToString()
        };
      }

      return result;
    }

    public ShareItemResponse IncreaseShareUsage(ShareItem item)
    {
      var shareItemResponse = new ShareItemResponse();

      var dbItem = _stateDBService.GetEntry(item.DBID);
      if(dbItem != null)
      {
        if(dbItem.RequestedTOPTRequests < dbItem.MaxTOPTRequests)
        {
          var vaultItem = _vaultService.GetItems().Where(x => x.id == item.VaultID).FirstOrDefault();
          if(vaultItem != null)
          {
            dbItem.RequestedTOPTRequests += 1;

            var toptCode = vaultItem.GetToptCode(_nTPService);
            shareItemResponse.TOPT = toptCode;
            shareItemResponse.UserName = vaultItem?.login?.username;
            shareItemResponse.Password = vaultItem?.login?.password;
            shareItemResponse.Success = true;
          }
        }

        if(dbItem.ExpireDate <= DateTime.Now)
          dbItem.Deleted = true;

        if(dbItem.RequestedTOPTRequests >= dbItem.MaxTOPTRequests)
          dbItem.Deleted = true;

        _stateDBService.AddOrUpdateEntry(dbItem);
      }

      return shareItemResponse;
    }
  }
}