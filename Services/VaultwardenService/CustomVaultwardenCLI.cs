using AuthCenter.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using OtpNet;
using OTP_Share.Services.NTPService;

namespace OTP_Share
{
  public class CustomVaultwardenCLI : IDisposable
  {
    private string mSession = "";
    private TimeSpan mCommandTimeout;

    public CustomVaultwardenCLI(string url, string clientId, string clientSecret, string password, TimeSpan timeout)
      : this(timeout)
    {
      var result = Login(url, clientId, clientSecret, password);
      if(!result.Result || string.IsNullOrEmpty(mSession))
      {
        throw new Exception("Can't logging into Vaultwarden. Check the client_id, client_secret and password and try again.");
      }
    }

    public CustomVaultwardenCLI(TimeSpan timeout)
      : this()
    {
      mCommandTimeout = timeout;
    }

    public CustomVaultwardenCLI()
    {
      mCommandTimeout = Timeout.InfiniteTimeSpan;
    }

    public CustomVaultResponse<bool> Login(string url, string clientId, string clientSecret, string password)
    {
      LogOut();

      var results = LogInUsingApi(url, clientId, clientSecret, password);

      var hasError = results.Any(x => x.IsError);
      var totalErrorMsg = string.Join(Environment.NewLine, results.Where(x => x.IsError).Select(x => x.ErrorMessage.TrimEnd('\r', '\n')));

      var lastStatus = results.LastOrDefault();
      mSession = !hasError ? lastStatus.CLIOutput : null;

      return new CustomVaultResponse<bool>()
      {
        Result = !hasError,
        ResultMsg = hasError ? totalErrorMsg : "Success",
        CmdResult = !string.IsNullOrEmpty(mSession)
      };
    }

    public CustomVaultResponse<IEnumerable<Item>> ListItems()
    {
      string cmd = "list items --session \"" + mSession + "\"";
      var cmdResult = IssueCLICommand(cmd, mCommandTimeout);

      var result = CustomVaultResponse<IEnumerable<Item>>.CreateFrom(cmdResult);
      return result;
    }

    public CustomVaultResponse<string> LogOut()
    {
      var cmdResult = IssueCLICommand("logout", mCommandTimeout);

      var result = CustomVaultResponse<string>.CreateFrom(cmdResult);
      return result;
    }

    public CustomVaultResponse<Item> GetItem(string id_or_name)
    {
      string cmd = $"get item {id_or_name} --session \"{mSession}\"";
      var cmdResult = IssueCLICommand(cmd, mCommandTimeout);

      var result = CustomVaultResponse<Item>.CreateFrom(cmdResult);
      return result;
    }

    private IEnumerable<RawCliResponse> LogInUsingApi(string url, string clientId, string clientSecret, string password)
    {
      List<RawCliResponse> results = new List<RawCliResponse>();

      var envVariables = new string[]
      {
        $"BW_CLIENTID={clientId}",
        $"BW_CLIENTSECRET={clientSecret}"
      };

      var commands = new List<string>()
      {
        $"config server {url}",
        $"login --apikey",
        $"unlock {password} --raw"
      };

      foreach(string command in commands)
      {
        var cmdResult = IssueCLICommand(command, mCommandTimeout, envVariables);
        results.Add(cmdResult);
      }

      return results;
    }

    private RawCliResponse IssueCLICommand(string cmd, TimeSpan timeout, params string[] envVariables)
    {
      string bWBinaryFilePath = GetBWBinaryFilePath();
      StringBuilder output = new StringBuilder();
      StringBuilder error = new StringBuilder();
      Process process = new Process();
      process.StartInfo.FileName = bWBinaryFilePath;
      process.StartInfo.Arguments = cmd ?? "";
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.ErrorDialog = false;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardInput = false;

      if(envVariables != null && envVariables.Length > 0)
      {
        foreach(string envVar in envVariables)
        {
          var keyValue = envVar.Split('=');
          if(keyValue.Length == 2)
          {
            process.StartInfo.Environment[keyValue[0]] = keyValue[1];
          }
        }
      }

      process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs eargs)
      {
        if(eargs.Data != null)
        {
          error.AppendLine(eargs.Data);
        }
      };
      process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs eargs)
      {
        if(eargs.Data != null)
        {
          output.AppendLine(eargs.Data);
        }
      };
      process.Start();
      process.BeginErrorReadLine();
      process.BeginOutputReadLine();
      var processExit = process.WaitForExit(timeout);

      if(!processExit)
        error.AppendLine("Process timeout");

      var errorStr = error.ToString();
      var result = new RawCliResponse
      {
        IsError = !processExit || !string.IsNullOrEmpty(errorStr),
        ErrorMessage = errorStr,
        CLIOutput = output.ToString()
      };

      return result;
    }

    private string GetBWBinaryFilePath()
    {
      string executableName = (Environment.OSVersion.Platform.ToString().StartsWith("Win") ? "bw.exe" : "bw");

      var fullPath = Path.Combine(AppContext.BaseDirectory, executableName);
      if(!File.Exists(fullPath))
      {
        throw new Exception(executableName + " not found in current directory. Before start, please download the last version of Bitwarden CLI (BW) from https://bitwarden.com/help/cli/");
      }

      return fullPath;
    }

    #region IDisposable Support

    private bool mIsDisposed;

    protected virtual void Dispose(bool disposing)
    {
      if(!mIsDisposed)
      {
        if(disposing)
        {
          // Dispose managed state (managed objects)
          LogOut();
        }

        // Free unmanaged resources (unmanaged objects) and override finalizer
        // Set large fields to null
        mIsDisposed = true;
      }
    }

    // Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~CustomVaultwardenCLI()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: false);
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
  }

  public static class CustomVaultItemExtensions
  {
    public static Totp GetTopt(this Item item)
    {
      Totp resultTotp = null;

      if(item != null && item.login != null && item.login.totp != null && item.login.totp.GetType() == typeof(string))
      {
        var normalizedTotpString = item.login.totp.ToString().Replace(" ", "");

        var secret = Base32Encoding.ToBytes(normalizedTotpString);
        resultTotp = new Totp(secret);
      }

      return resultTotp;
    }

    public static CustomToptCodeResponse GetToptCode(this Item item, INTPService service)
    {
      var result = new CustomToptCodeResponse();

      Totp resultTotp = GetTopt(item);

      if(resultTotp != null)
      {
        var preciseTime = service.GetUTC();
        var codes = new List<CustomToptCode>();

        var currentTOPT = new CustomToptCode()
        {
          WindowStart = resultTotp.WindowStart(preciseTime),
          TOTPCode = resultTotp.ComputeTotp(preciseTime),
          RemainingSecs = resultTotp.RemainingSeconds(preciseTime)
        };
        codes.Add(currentTOPT);

        var preciseTimeNextWindow = currentTOPT.WindowStart.AddSeconds(30 + 1);
        codes.Add(new CustomToptCode()
        {
          WindowStart = resultTotp.WindowStart(preciseTimeNextWindow),
          TOTPCode = resultTotp.ComputeTotp(preciseTimeNextWindow),
          RemainingSecs = resultTotp.RemainingSeconds(preciseTimeNextWindow)
        });

        if(currentTOPT.RemainingSecs < 15)
        {
          preciseTimeNextWindow = preciseTimeNextWindow = currentTOPT.WindowStart.AddSeconds(60 + 1);
          codes.Add(new CustomToptCode()
          {
            WindowStart = resultTotp.WindowStart(preciseTimeNextWindow),
            TOTPCode = resultTotp.ComputeTotp(preciseTimeNextWindow),
            RemainingSecs = resultTotp.RemainingSeconds(preciseTimeNextWindow)
          });
        }

        result.TOPTCodes = codes;
      }

      return result;
    }
  }

  public class CustomToptCodeResponse
  {
    public IEnumerable<CustomToptCode> TOPTCodes { get; set; }
  }

  public class CustomToptCode
  {
    public DateTime WindowStart { get; set; }
    public string TOTPCode { get; set; }
    public int RemainingSecs { get; set; }
  }

  public class CustomVaultResponse<T>
  {
    public bool Result { get; set; }
    public string ResultMsg { get; set; }
    public T CmdResult { get; set; }

    public static CustomVaultResponse<T> CreateFrom(RawCliResponse response)
    {
      var isJson = ValidateJSON(response.CLIOutput);
      var isString = typeof(T) == typeof(string);

      T result = default(T);

      if(isString)
        result = (T)Convert.ChangeType(response.CLIOutput, typeof(T));
      else if(isJson)
        result = JsonConvert.DeserializeObject<T>(response.CLIOutput);

      return new CustomVaultResponse<T>
      {
        Result = !response.IsError,
        ResultMsg = response.IsError ? response.ErrorMessage : (isJson ? "Success" : response.CLIOutput),
        CmdResult = result
      };
    }

    private static bool ValidateJSON(string s)
    {
      try
      {
        JToken.Parse(s);
        return true;
      }
      catch(JsonReaderException value)
      {
        Trace.WriteLine(value);
        return false;
      }
    }
  }

  public class RawCliResponse
  {
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public string CLIOutput { get; set; }
  }
}