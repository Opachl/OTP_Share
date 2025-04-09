using OTP_Share.Services.NTPService;
using OTP_Share.Services.StateDBService;
using OTP_Share.Services.VaultwardenService;

namespace OTP_Share.Services
{
  public class DatalayerService
  {
    private readonly ILogger<DatalayerService> _logger;

    private readonly IStateDBService _stateDBService;
    private readonly IVaultService _vaultService;
    private readonly INTPService _nTPService;

    public DatalayerService(IStateDBService stateDB, IVaultService vaultService, INTPService nTPService, ILogger<DatalayerService> logger)
    {
      _stateDBService = stateDB;
      _vaultService = vaultService;
      _nTPService = nTPService;
      _logger = logger;
    }

    public void Init()
    {
      _logger.LogInformation("Initializing DatalayerService...");
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

      if(result == null)
        _logger.LogWarning("Failed to register new share for Vault ID: {VaultID}", vaultID);
      else
        _logger.LogInformation("Successfully registered new share: {ShareItem}", result);

      return result;
    }

    public ShareItemResponse IncreaseShareUsage(ShareItem item)
    {
      _logger.LogInformation("IncreaseShareUsage called for ShareItem: {ShareItem}", item);

      var shareItemResponse = new ShareItemResponse();

      var dbItem = _stateDBService.GetEntry(item.DBID);
      if(dbItem != null)
      {
        _logger.LogInformation("DB entry found for ID: {DBID}", item.DBID);

        if(dbItem.RequestedTOPTRequests < dbItem.MaxTOPTRequests)
        {
          _logger.LogInformation("RequestedTOPTRequests ({Requested}) is less than MaxTOPTRequests ({Max}) for DBID: {DBID}",
            dbItem.RequestedTOPTRequests, dbItem.MaxTOPTRequests, item.DBID);

          var vaultItem = _vaultService.GetItems().Where(x => x.id == item.VaultID).FirstOrDefault();
          if(vaultItem != null)
          {
            _logger.LogInformation("Vault item found for VaultID: {VaultID}", item.VaultID);

            dbItem.RequestedTOPTRequests += 1;
            _logger.LogInformation("Incremented RequestedTOPTRequests to {Requested} for DBID: {DBID}",
              dbItem.RequestedTOPTRequests, item.DBID);

            var toptCode = vaultItem.GetToptCode(_nTPService);
            shareItemResponse.TOPT = toptCode;
            shareItemResponse.UserName = vaultItem?.login?.username;
            shareItemResponse.Password = vaultItem?.login?.password;
            shareItemResponse.Success = true;

            _logger.LogInformation("Generated TOPT code and updated ShareItemResponse for DBID: {DBID}", item.DBID);
          }
          else
          {
            _logger.LogWarning("No vault item found for VaultID: {VaultID}", item.VaultID);
          }
        }
        else
        {
          _logger.LogWarning("RequestedTOPTRequests ({Requested}) has reached or exceeded MaxTOPTRequests ({Max}) for DBID: {DBID}",
            dbItem.RequestedTOPTRequests, dbItem.MaxTOPTRequests, item.DBID);
        }

        if(dbItem.ExpireDate <= DateTime.Now)
        {
          dbItem.Deleted = true;
          _logger.LogInformation("Marked DB entry as deleted due to expiration for DBID: {DBID}", item.DBID);
        }

        if(dbItem.RequestedTOPTRequests >= dbItem.MaxTOPTRequests)
        {
          dbItem.Deleted = true;
          _logger.LogInformation("Marked DB entry as deleted due to reaching max requests for DBID: {DBID}", item.DBID);
        }

        _stateDBService.AddOrUpdateEntry(dbItem);
        _logger.LogInformation("Updated DB entry for DBID: {DBID}", item.DBID);
      }
      else
      {
        _logger.LogWarning("No DB entry found for ID: {DBID}", item.DBID);
      }

      _logger.LogInformation("IncreaseShareUsage completed for ShareItem: {ShareItem}", item);
      return shareItemResponse;
    }
  }
}