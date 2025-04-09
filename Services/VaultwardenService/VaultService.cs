using AuthCenter.Models;
using OTP_Share.Services.EnvironmentService;

namespace OTP_Share.Services.VaultwardenService
{
  public class VaultService : IVaultService
  {
    private readonly IEnvironmentService _EnvSrv;

    private CustomVaultwardenCLI _CLI;

    public VaultService(IEnvironmentService environment)
    {
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
        var loginResponse = _CLI.Login(_EnvSrv.VaultwardenURL, _EnvSrv.VaultwardenClientId, _EnvSrv.VaultwardenClientSecret, _EnvSrv.VaultwardenUserPassword);
        if(loginResponse != null && loginResponse.Result)
        {
          var itemsResponse = _CLI.ListItems();

          if(itemsResponse != null && itemsResponse.CmdResult != null)
            result = itemsResponse.CmdResult;
        }
      }
      catch(Exception ex)
      {
      }

      return result;
    }
  }
}