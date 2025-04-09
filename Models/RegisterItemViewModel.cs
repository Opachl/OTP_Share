namespace OTP_Share.Models
{
  public class RegisterItemViewModel
  {
    public string ItemId { get; set; }
    public int MaxUsages { get; set; }
    public DateTime ExpireDate { get; set; }
  }
}