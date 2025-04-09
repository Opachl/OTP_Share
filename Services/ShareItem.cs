namespace OTP_Share.Services
{
  public class ShareItem
  {
    public long DBID { get; set; }
    public string VaultID { get; set; }
    public string GroupID { get; set; }
  }

  public class ShareItemResponse
  {
    public bool Success { get; set; }
    public CustomToptCodeResponse TOPT { get; set; }

    public string UserName { get; set; }
    public string Password { get; set; }
  }
}