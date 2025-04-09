using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTP_Share.Services.StateDBService.Model
{
  public class DBSharedLink
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public Guid ShareGroupID { get; set; }
    public string VaultItemId { get; set; }
    public string DisplayName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public int MaxTOPTRequests { get; set; }
    public int RequestedTOPTRequests { get; set; }
    public bool Deleted { get; set; }
  }
}