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
      _CLI = new CustomVaultwardenCLI(_EnvSrv.VaultwardenCLITimeout);
    }

    public IEnumerable<Item> GetItems()
    {
      IEnumerable<Item> result = null;
      try
      {
        result = Get<IEnumerable<Item>>($"{nameof(VaultService)}_{nameof(GetItems)}");
        if(result == null || !result.Any())
        {
          _logger.LogInformation("Cache miss for {CacheKey}. Attempting to log in.", $"{nameof(VaultService)}_{nameof(GetItems)}");

          var loginResponse = _CLI.Login(_EnvSrv.VaultwardenURL, _EnvSrv.VaultwardenClientId, _EnvSrv.VaultwardenClientSecret, _EnvSrv.VaultwardenUserPassword);
          if(loginResponse != null)
            _logger.LogInformation("Login response received. Result: {Result}, {ResultMsg}", loginResponse.Result, loginResponse.ResultMsg);
          else
            _logger.LogWarning("Login response is null.");

          if(loginResponse != null && loginResponse.Result)
          {
            var itemsResponse = _CLI.ListItems();

            if(itemsResponse != null)
              _logger.LogInformation("Items response received. Command Result Count: {Count}", itemsResponse?.CmdResult?.Count() ?? 0);
            else
              _logger.LogWarning("Items response is null.");

            if(itemsResponse != null && itemsResponse.CmdResult != null)
            {
              result = itemsResponse.CmdResult;
              Set($"{nameof(VaultService)}_{nameof(GetItems)}", result);
              _logger.LogInformation("Items successfully cached with key {CacheKey}.", $"{nameof(VaultService)}_{nameof(GetItems)}");
            }
          }
          else
            _logger.LogWarning("Login failed. Unable to retrieve items.");
        }
        else
          _logger.LogInformation("Cache hit for {CacheKey}. Returning cached items.", $"{nameof(VaultService)}_{nameof(GetItems)}");
      }
      catch(Exception ex)
      {
        _logger.LogError(ex, $"Error in {nameof(VaultService)}.{nameof(GetItems)}: {ex.Message}");
      }

      if(result == null)
        result = new List<Item>();

      return result;
    }
  }
}