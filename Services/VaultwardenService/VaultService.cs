using AuthCenter.Models;
using OTP_Share.Services.EnvironmentService;

namespace OTP_Share.Services.VaultwardenService
{
  public class VaultService : CacheBaseService, IVaultService
  {
    private readonly ILogger<VaultService> _logger;

    private readonly IEnvironmentService _EnvSrv;
    private CustomVaultwardenCLI _CLI;

    public VaultService(ILogger<VaultService> logger, IEnvironmentService environment)
      : base(logger)
    {
      _logger = logger;
      _EnvSrv = environment;
      InitIF();
    }

    private void InitIF()
    {
      _CLI = new CustomVaultwardenCLI(TimeSpan.FromSeconds(10));
    }

    public IEnumerable<Item> GetItems()
    {
      IEnumerable<Item> result = new List<Item>();

      try
      {
        result = Get<IEnumerable<Item>>($"{nameof(VaultService)}_{nameof(GetItems)}");
        if(result == null || !result.Any())
        {
          var loginResponse = _CLI.Login(_EnvSrv.VaultwardenURL, _EnvSrv.VaultwardenClientId, _EnvSrv.VaultwardenClientSecret, _EnvSrv.VaultwardenUserPassword);
          if(loginResponse != null && loginResponse.Result)
          {
            var itemsResponse = _CLI.ListItems();

            if(itemsResponse != null && itemsResponse.CmdResult != null)
            {
              result = itemsResponse.CmdResult;
              Set($"{nameof(VaultService)}_{nameof(GetItems)}", result);
            }
          }
        }
      }
      catch(Exception ex)
      {
        _logger.LogError(ex, $"Error in {nameof(VaultService)}.{nameof(GetItems)}: {ex.Message}");
      }

      return result;
    }
  }
}