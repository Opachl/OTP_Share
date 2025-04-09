using GuerrillaNtp;
using OTP_Share.Services.EnvironmentService;

namespace OTP_Share.Services.NTPService
{
  public class NTPService : INTPService
  {
    private readonly IEnvironmentService _EnvSrv;
    private NtpClient _CLI;

    public NTPService(IEnvironmentService environment)
    {
      _EnvSrv = environment;
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
          return client.Query();
        }
        catch
        {
          Thread.Sleep(delay);
          delay = 2 * delay;
        }
      }
    }
  }
}