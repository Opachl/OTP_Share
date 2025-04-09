using AuthCenter.Models;

namespace OTP_Share.Services.VaultwardenService
{
  public interface IVaultService
  {
    IEnumerable<Item> GetItems();
  }
}