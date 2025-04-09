namespace OTP_Share.Services.EnvironmentService
{
  public interface IEnvironmentService
  {
    string VaultwardenURL { get; }
    string VaultwardenUserPassword { get; }
    string VaultwardenClientId { get; }
    string VaultwardenClientSecret { get; }

    public string DB_ConnectionString { get; }

    string NTPDEFAULTPool { get; }
    bool AdminPageEnabled { get; }
    string AdminUser { get; }
    string AdminPassword { get; }
    //bool NTPSYNCWithSystem { get; }
  }
}