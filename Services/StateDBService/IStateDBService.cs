namespace OTP_Share.Services.StateDBService
{
  public interface IStateDBService
  {
    void AddOrUpdateEntry(Model.DBSharedLink entry);

    Model.DBSharedLink GetEntry(long id);

    IEnumerable<Model.DBSharedLink> GetValidEntrys();

    IEnumerable<Model.DBSharedLink> GetValidEntrys(Guid groupID);
  }
}