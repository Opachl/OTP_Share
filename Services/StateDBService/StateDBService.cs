using Microsoft.EntityFrameworkCore;
using OTP_Share.Services.EnvironmentService;

namespace OTP_Share.Services.StateDBService
{
  public class StateDBService : IStateDBService
  {
    private readonly ILogger<StateDBService> _Logger;
    private readonly IEnvironmentService _EnvSrv;

    public StateDBService(ILogger<StateDBService> logger, IServiceProvider serviceProvider, IEnvironmentService environment)
    {
      _Logger = logger;
      _EnvSrv = environment;

      Init();
    }

    public void Init()
    {
      using(var db = new StateDBContext(_EnvSrv))
      {
        while(!db.IsDatabaseConnected())
        {
          _Logger.LogWarning("[StateDBService] waiting for db ready...");
        }

        _Logger.LogInformation("[StateDBService] db ready...");
        var pendingMigrations = db.Database.GetPendingMigrations().ToList();
        if(pendingMigrations.Count > 0)
        {
          pendingMigrations.Insert(0, "[StateDBService] Pending Migrations:");
          _Logger.LogInformation(string.Join(Environment.NewLine, pendingMigrations));

          db.Database.Migrate();

          _Logger.LogInformation("[StateDBService] Migrations done");
        }
      }
    }

    public Model.DBSharedLink GetEntry(long id)
    {
      using(var db = new StateDBContext(_EnvSrv))
      {
        var cEntry = db.SharedToptLinks.Where(x => x.Id == id).FirstOrDefault();
        return cEntry;
      }
    }

    public void AddOrUpdateEntry(Model.DBSharedLink entry)
    {
      using(var db = new StateDBContext(_EnvSrv))
      {
        var cEntry = db.SharedToptLinks.Where(x => x.Id == entry.Id).FirstOrDefault() ?? entry;

        if(cEntry.Id == 0)
          db.Add(cEntry);
        else
        {
          cEntry.DisplayName = entry.DisplayName;
          cEntry.RequestedTOPTRequests = entry.RequestedTOPTRequests;
          cEntry.Deleted = entry.Deleted;

          db.Update(cEntry);
        }

        var dbResult = db.SaveChanges() != 0;
      }
    }

    public IEnumerable<Model.DBSharedLink> GetValidEntrys()
    {
      using(var db = new StateDBContext(_EnvSrv))
      {
        var notExpiredItems = db.SharedToptLinks
          .Where(x => !x.Deleted && x.ExpireDate > DateTime.Now)
          .ToList();

        return notExpiredItems;
      }
    }

    public IEnumerable<Model.DBSharedLink> GetValidEntrys(Guid groupID)
    {
      using(var db = new StateDBContext(_EnvSrv))
      {
        var notExpiredItems = db.SharedToptLinks
          .Where(x => x.ShareGroupID == groupID && !x.Deleted && x.ExpireDate > DateTime.Now)
          .ToList();

        return notExpiredItems;
      }
    }
  }
}