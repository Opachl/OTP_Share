namespace OTP_Share.Services.EnvironmentService
{
  public class EnvironmentVariablesSrv : IEnvironmentService
  {
    private readonly ILogger<EnvironmentVariablesSrv> _logger;

    private const string KEY_ADMINPAGE = "ENABLE_ADMIN";
    private const string KEY_ADMINUSER = "ADMIN_USER";
    private const string KEY_ADMINPASS = "ADMIN_PASS";

    private const string KEY_VAULTURL = "VW_URL";

    private const string KEY_VAULTPASSWORD = "VW_USERPW";
    private const string KEY_CLIENTID = "VW_CLIENTID";
    private const string KEY_CLIENTSECRET = "VW_CLIENTSECRET";
    private const string KEY_CLICOMMANDTIMEOUT = "VW_TIMEOUT";

    private const string KEY_DBSERVER = "DB_SERVER";
    private const string KEY_DBSERVERPORT = "DB_SERVERPORT";
    private const string KEY_DBSUSER = "DB_USER";
    private const string KEY_DBPASSWORD = "DB_PASSWORD";
    private const string KEY_DB = "DB_DATABASE";

    private const string KEY_NTPDEFAULTPool = "NTP_DEFAULTPool";
    private const string KEY_NTPSYNCWithSystem = "NTP_SYNCWithSystem";

    public EnvironmentVariablesSrv(ILogger<EnvironmentVariablesSrv> logger)
    {
      _logger = logger;

      if(string.IsNullOrEmpty(EnsureVar(KEY_CLICOMMANDTIMEOUT)))
        VaultwardenCLITimeout = TimeSpan.FromSeconds(10);

      if(string.IsNullOrEmpty(NTPDEFAULTPool))
      {
        NTPDEFAULTPool = "de.pool.ntp.org";
        _logger.LogInformation("Default NTP pool set to: {NTPDEFAULTPool}", NTPDEFAULTPool);
      }
    }

    public static bool IsAdminPageEnabled => Environment.GetEnvironmentVariable(KEY_ADMINPAGE) == "1" || Environment.GetEnvironmentVariable(KEY_ADMINPAGE) == "true";

    public bool AdminPageEnabled
    {
      set { EnsureBoolVar(KEY_ADMINPAGE, value); }
      get { return EnsureBoolVar(KEY_ADMINPAGE); }
    }

    public string AdminUser
    {
      set { EnsureVar(KEY_ADMINUSER, value); }
      get { return EnsureVar(KEY_ADMINUSER); }
    }

    public string AdminPassword
    {
      set { EnsureVar(KEY_ADMINPASS, value); }
      get { return EnsureVar(KEY_ADMINPASS); }
    }

    public string VaultwardenURL
    {
      set { EnsureVar(KEY_VAULTURL, value); }
      get { return EnsureVar(KEY_VAULTURL); }
    }

    public string VaultwardenUserPassword
    {
      set { EnsureVar(KEY_VAULTPASSWORD, value); }
      get { return EnsureVar(KEY_VAULTPASSWORD); }
    }

    public string VaultwardenClientId
    {
      set { EnsureVar(KEY_CLIENTID, value); }
      get { return EnsureVar(KEY_CLIENTID); }
    }

    public string VaultwardenClientSecret
    {
      set { EnsureVar(KEY_CLIENTSECRET, value); }
      get { return EnsureVar(KEY_CLIENTSECRET); }
    }

    public TimeSpan VaultwardenCLITimeout
    {
      set { EnsureTimespanVar(KEY_CLICOMMANDTIMEOUT, value); }
      get { return EnsureTimespanVar(KEY_CLICOMMANDTIMEOUT); }
    }

    public string DB_SERVER
    {
      set { EnsureVar(KEY_DBSERVER, value); }
      get { return EnsureVar(KEY_DBSERVER); }
    }

    public string DB_SERVER_PORT
    {
      set { EnsureVar(KEY_DBSERVERPORT, value); }
      get { return EnsureVar(KEY_DBSERVERPORT); }
    }

    public string DB_USER
    {
      set { EnsureVar(KEY_DBSUSER, value); }
      get { return EnsureVar(KEY_DBSUSER); }
    }

    public string DB_PASSWORD
    {
      set { EnsureVar(KEY_DBPASSWORD, value); }
      get { return EnsureVar(KEY_DBPASSWORD); }
    }

    public string DB_DB
    {
      set { EnsureVar(KEY_DB, value); }
      get { return EnsureVar(KEY_DB); }
    }

    public string DB_ConnectionString
    {
      get
      {
        var connectionString = $"Server={DB_SERVER};Port={DB_SERVER_PORT};Database={DB_DB};User Id={DB_USER};Password={DB_PASSWORD};";
        return connectionString;
      }
    }

    public string NTPDEFAULTPool
    {
      set { EnsureVar(KEY_NTPDEFAULTPool, value); }
      get { return EnsureVar(KEY_NTPDEFAULTPool); }
    }

    public bool NTPSYNCWithSystem
    {
      set { EnsureBoolVar(KEY_NTPSYNCWithSystem, value); }
      get { return EnsureBoolVar(KEY_NTPSYNCWithSystem); }
    }

    public bool EnsureBoolVar(string variable, bool? value = null)
    {
      // Use EnsureVar to set or get the environment variable as a string
      var strVal = EnsureVar(variable, value.ToString().ToLower());

      // If the value is null, set the variable to false
      if(string.IsNullOrEmpty(strVal))
        value = false;

      // Try to parse the string value to a boolean
      if(bool.TryParse(strVal, out var result))
        return result;
      else
      {
        // If parsing fails, return the default value (false)
        _logger.LogWarning("Invalid boolean value for {Variable}: {Value}. Defaulting to false.", variable, strVal);
        return false;
      }
    }

    public TimeSpan EnsureTimespanVar(string variable, TimeSpan? value = null)
    {
      // Use EnsureVar to set or get the environment variable as a string
      var strVal = EnsureVar(variable, value.ToString());

      // If the value is null, set the variable to the default (10 seconds)
      if(string.IsNullOrEmpty(strVal))
        value = TimeSpan.FromSeconds(10);

      // Try to parse the string value to a TimeSpan
      if(TimeSpan.TryParse(strVal, out var result))
        return result;
      else
      {
        // If parsing fails, return the default value (10 seconds)
        _logger.LogWarning("Invalid TimeSpan value for {Variable}: {Value}. Defaulting to 10 seconds.", variable, strVal);
        return TimeSpan.FromSeconds(10);
      }
    }

    public string EnsureVar(string variable, string value = null)
    {
      // Get the current state of the environment variable
      var currentVariableState = Environment.GetEnvironmentVariable(variable);

      // If the value is null, set the variable to an empty string
      if(string.IsNullOrEmpty(value) && string.IsNullOrEmpty(currentVariableState))
        Environment.SetEnvironmentVariable(variable, value);

      // If the value is different, update the environment variable and log it
      if(currentVariableState != value && !string.IsNullOrEmpty(value))
      {
        Environment.SetEnvironmentVariable(variable, value);
        _logger.LogInformation("Environment variable {Variable} set to: {Value}", variable, value);
      }

      // Return the current state of the environment variable
      return Environment.GetEnvironmentVariable(variable);
    }
  }
}