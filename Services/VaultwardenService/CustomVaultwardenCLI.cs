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
      var totalErrorMsg = string.Join(Environment.NewLine, results.Where(x => x.IsError).Select(x => $"C_{x.CmdID} -> {x.ErrorMessage.TrimEnd('\r', '\n')}"));
      var totalCLIMsg = string.Join(Environment.NewLine, results.Select(x => $"C_{x.CmdID} -> {x.CLIOutput.TrimEnd('\r', '\n')}"));

      var lastStatus = results.LastOrDefault();
      mSession = !hasError ? lastStatus.CLIOutput : null;

      return new CustomVaultResponse<bool>()
      {
        Result = !hasError,
        ResultMsg = hasError ? $"Error: {totalErrorMsg}, CLI: {totalCLIMsg}" : "Success",
        CmdResult = !string.IsNullOrEmpty(mSession)
      };
    }

    public CustomVaultResponse<IEnumerable<Item>> ListItems()
    {
      var cmd = "bw list items --session \"" + mSession + "\"";
      var cmdResult = IssueCLIShellCommand(cmd, mCommandTimeout);

      var result = CustomVaultResponse<IEnumerable<Item>>.CreateFrom(cmdResult);
      return result;
    }

    public CustomVaultResponse<string> LogOut()
    {
      var cmd = "bw logout";
      var cmdResult = IssueCLIShellCommand(cmd, mCommandTimeout);

      var result = CustomVaultResponse<string>.CreateFrom(cmdResult);
      return result;
    }

    public CustomVaultResponse<Item> GetItem(string id_or_name)
    {
      var cmd = $"bw get item {id_or_name} --session \"{mSession}\"";
      var cmdResult = IssueCLIShellCommand(cmd, mCommandTimeout);

      var result = CustomVaultResponse<Item>.CreateFrom(cmdResult);
      return result;
    }

    private IEnumerable<RawCliResponse> loLogInUsingApi(string url, string clientId, string clientSecret, string password)
    {
      List<RawCliResponse> results = new List<RawCliResponse>();

      //var envVariables = new string[]
      //{
      //  $"BW_CLIENTID={clientId}",
      //  $"BW_CLIENTSECRET={clientSecret}"
      //};

      var correctedURL = EnsureValidServerURL(url);
      var commands = new List<string>()
      {
        $"bw config server {correctedURL}",
        $"BW_CLIENTID={clientId} BW_CLIENTSECRET={clientSecret} bw login --apikey",
        $"bw unlock {password} --raw"
      };

      int cmdIndex = 0;
      foreach(string command in commands)
      {
        var cmdResult = IssueCLIShellCommand(command, mCommandTimeout);
        cmdResult.CmdID = cmdIndex;
        results.Add(cmdResult);
        cmdIndex++;
      }

      return results;
    }

    private RawCliResponse IssueCLIShellCommand(string cmd, TimeSpan timeout, params string[] envVariables)
    {
      StringBuilder output = new StringBuilder();
      StringBuilder error = new StringBuilder();
      Process process = new Process();

      // Configure ProcessStartInfo for shell execution
      process.StartInfo.WorkingDirectory = AppContext.BaseDirectory;
      process.StartInfo.FileName = "/bin/bash"; // Use bash for shell execution on Linux
      process.StartInfo.Arguments = $"-c \"{cmd}\""; // Pass the command to bash
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true; // Do not create a new window
      process.StartInfo.ErrorDialog = false;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.RedirectStandardOutput = true;

      // Add environment variables if provided
      if(envVariables != null && envVariables.Length > 0)
      {
        foreach(string envVar in envVariables)
        {
          var keyValue = envVar.Split('=');
          if(keyValue.Length == 2)
            process.StartInfo.Environment[keyValue[0]] = keyValue[1];
        }
      }

      process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs eargs)
      {
        if(eargs.Data != null)
          error.AppendLine(eargs.Data);
      };

      process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs eargs)
      {
        if(eargs.Data != null)
          output.AppendLine(eargs.Data);
      };

      // Start the process and capture output
      process.Start();
      process.BeginErrorReadLine();
      process.BeginOutputReadLine();
      var processExit = process.WaitForExit((int)timeout.TotalMilliseconds);

      if(!processExit)
        error.AppendLine("Process timeout");

      // Read the output and error streams
      if(process.ExitCode != 0)
        error.AppendLine($"Process exited with code {process.ExitCode}");

      var errorStr = error.ToString();
      var result = new RawCliResponse
      {
        IsError = !processExit || !string.IsNullOrEmpty(errorStr) || process.ExitCode != 0,
        ErrorMessage = error.ToString(),
        CLIOutput = output.ToString()
      };

      return result;
    }

    private string EnsureValidServerURL(string url)
    {
      if(string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be null or empty.", nameof(url));

      // Ensure the URL starts with "https://"
      if(!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
      {
        if(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
          url = "https://" + url.Substring("http://".Length);
        else
          url = "https://" + url;
      }

      // Ensure the URL ends with "/"
      if(!url.EndsWith("/"))
        url += "/";

      return url;
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
    public int CmdID { get; set; }

    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public string CLIOutput { get; set; }
  }
}