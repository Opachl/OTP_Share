namespace OTP_Share.Models
{
  public class ItemViewModel
  {
    public string Id { get; set; }
    public string Title { get; set; }
    public bool HasTOPT { get; set; }
    public IEnumerable<Guid> ActiveGroupIDs { get; set; }
  }
}