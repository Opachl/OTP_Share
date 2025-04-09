namespace OTP_Share.Models
{
  public class GroupItemViewModel
  {
    public GroupItemViewModel(Guid groupID, long stateDBId, string vaultItemId)
    {
      _mID = $"%g{groupID}%s{stateDBId}%v{vaultItemId}";
    }

    private string _mID;

    public string Id
    {
      get { return _mID; }
    }

    public string DisplayName { get; set; }

    public DateTime ExpireDate { get; set; }
    public int MaxTOPTRequests { get; set; }
    public int RequestedTOPTRequests { get; set; }

    public static string GetGroupID(string itemID)
    {
      return GetSection(itemID, "g") == "" ? "" : GetSection(itemID, "g");
    }

    public static long GetStateDBId(string itemID)
    {
      return GetSection(itemID, "s") == "" ? 0 : long.Parse(GetSection(itemID, "s"));
    }

    public static string GetVaultItemId(string itemID)
    {
      return GetSection(itemID, "v") == "" ? "" : GetSection(itemID, "v");
    }

    private static string GetSection(string itemID, string section)
    {
      var sectionID = $"{section}";
      var sections = itemID.Split("%", StringSplitOptions.RemoveEmptyEntries);
      var requestedSection = sections.FirstOrDefault(x => x.StartsWith(sectionID));
      var requestedSectionValue = requestedSection.Replace(sectionID, "");

      return requestedSectionValue;
    }
  }
}