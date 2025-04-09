using Microsoft.EntityFrameworkCore;
using OTP_Share.Services.EnvironmentService;
using OTP_Share.Services.StateDBService.Model;
using System.Reflection.Emit;
using System.Security.Principal;

namespace OTP_Share.Services.StateDBService
{
  public class StateDBContext : DbContext
  {
    private readonly IEnvironmentService _EnvSrv;

    public StateDBContext(IEnvironmentService environment)
    {
      _EnvSrv = environment;
    }

#if DEBUG_LOCAL
    private Microsoft.Data.Sqlite.SqliteConnection mConnection;

    private static Microsoft.Data.Sqlite.SqliteConnection InitializeSQLiteConnection(string databaseFile)
    {
      var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
      {
        DataSource = databaseFile
        //Password = "myPW" // PRAGMA key is being sent from EF Core directly after opening the connection
      };

      return new Microsoft.Data.Sqlite.SqliteConnection(connectionString.ToString());
    }

#endif

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if  DEBUG_LOCAL
      var settingsDIR = Utility.GetSettingsRootDir();
      var fp = Path.Combine(settingsDIR, "state.db");
      mConnection ??= InitializeSQLiteConnection(fp);
      optionsBuilder.UseSqlite(mConnection);
#else
      var connectionString = _EnvSrv.DB_ConnectionString;

#if ADD_Migration
      connectionString = $"Server=localhost;Port=3306;Database=db01;User Id=dev;Password=Qh3hgLERJyBVi;";
#endif

      var version = ServerVersion.AutoDetect(connectionString);
      optionsBuilder.UseMySql(connectionString, version);
      // The following three options help with debugging, but should
      // be changed or removed for production.
      //.LogTo(Console.WriteLine, LogLevel.Information)
      //.EnableSensitiveDataLogging()
      //.EnableDetailedErrors();
#endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
    }

    public DbSet<DBSharedLink> SharedToptLinks { get; set; }

    public bool IsDatabaseConnected()
    {
      try
      {
        Database.OpenConnection();
        Database.CloseConnection();
        return true; // Connection is available
      }
      catch(Exception ex)
      {
        return false; // Connection is not available
      }
    }
  }
}