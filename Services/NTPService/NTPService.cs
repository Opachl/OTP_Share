using GuerrillaNtp;
using OTP_Share.Services.EnvironmentService;

namespace OTP_Share.Services.NTPService
{
  public class NTPService : INTPService
  {
    private readonly ILogger<NTPService> _logger;
    private readonly IEnvironmentService _EnvSrv;
    private NtpClient _CLI;

    public NTPService(IEnvironmentService environment, ILogger<NTPService> logger)
    {
      _EnvSrv = environment;
      _logger = logger;
      InitIF();
    }

    private void InitIF()
    {
      _CLI = new NtpClient(_EnvSrv.NTPDEFAULTPool);
      var clock = QueryWithBackoff(_CLI);

      //if(_EnvSrv.NTPSYNCWithSystem)
      //  SystemClock.SetSystemTimeUtc
    }

    public DateTime GetUTC()
    {
      var time = (_CLI.Last ?? NtpClock.LocalFallback).UtcNow.UtcDateTime;
      return time;
    }

    private NtpClock QueryWithBackoff(NtpClient client)
    {
      var delay = TimeSpan.FromSeconds(1);
      while(true)
      {
        try
        {
          _logger.LogInformation("Querying NTP server...");
          return client.Query();
        }
        catch(Exception ex)
        {
          _logger.LogWarning("NTP query failed. Retrying in {Delay} seconds. Error: {Error}", delay.TotalSeconds, ex.Message);
          Thread.Sleep(delay);
          delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60)); // Cap delay at 60 seconds
        }
      }
    }
  }
}