using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UberDeployer.Core.Domain;
using System.Text.RegularExpressions;
using System.Threading;
using UberDeployer.Core.Management.MsDeploy;

namespace UberDeployer.Core.Management.Iis
{
  // TODO IMM HI: RETRIES ARE NOT ENOUGH!

  public class MsDeployBasedIisManager : IIisManager
  {
    private readonly IMsDeploy _msDeploy;
    private const string _RemoteAppCmdExePath = "C:\\Windows\\system32\\inetsrv\\appcmd.exe";
    private const int _AppCmdRetriesCount = 9;

    private static readonly Regex _AppPoolInfoRegex = new Regex("APPPOOL \"(?<AppPoolName>[^\"]+)\" \\(MgdVersion:(?<AppPoolVersion>[^,]+),MgdMode:(?<AppPoolMode>[^,\\)]+)[^\\)]+\\)\\r?\\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _ExitCodeRegex = new Regex("exited with code '(?<ExitCode>0x[0-9a-fA-F]+)'", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    #region Constructor(s)

    public MsDeployBasedIisManager(IMsDeploy msDeploy)
    {
      if (msDeploy == null)
      {
        throw new ArgumentNullException("msDeploy", "Argument can't be null.");
      }
      _msDeploy = msDeploy;
    }

    #endregion

    #region IIisManager Members

    // TODO IMM HI: retries on call site?
    public IDictionary<string, IisAppPoolInfo> GetAppPools(string machineName)
    {
      if (string.IsNullOrEmpty(machineName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "machineName");
      }

      return GetAppPoolsImpl(machineName, _AppCmdRetriesCount, 0);
    }

    public bool AppPoolExists(string machineName, string appPoolName)
    {
      if (string.IsNullOrEmpty(machineName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "machineName");
      }

      if (string.IsNullOrEmpty(appPoolName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "appPoolName");
      }

      IDictionary<string, IisAppPoolInfo> appPools =
        GetAppPoolsImpl(machineName, _AppCmdRetriesCount, 0);

      return appPools.ContainsKey(appPoolName);
    }

    public void CreateAppPool(string machineName, IisAppPoolInfo appPoolInfo)
    {
      if (string.IsNullOrEmpty(machineName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "machineName");
      }

      if (appPoolInfo == null)
      {
        throw new ArgumentNullException("appPoolInfo");
      }

      string appCmdCommand =
        string.Format(
          "add apppool /name:\"{0}\" -managedRuntimeVersion:\"{1}\" -managedPipelineMode:\"{2}\"",
          appPoolInfo.Name,
          appPoolInfo.Version.ToString().Replace("_", "."),
          appPoolInfo.Mode);

      RunAppCmd(machineName, appCmdCommand);
    }

    public void SetAppPool(string machineName, string fullWebAppName, string appPoolName)
    {
      if (string.IsNullOrEmpty(machineName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "machineName");
      }

      if (string.IsNullOrEmpty(fullWebAppName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "fullWebAppName");
      }

      if (string.IsNullOrEmpty(appPoolName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "appPoolName");
      }

      if (!AppPoolExists(machineName, appPoolName))
      {
        throw new ArgumentException(string.Format("Couldn't set app's application pool to '{0}' because it doesn't exist on '{1}'.", appPoolName, machineName), "appPoolName");
      }

      string appCmdCommand =
        string.Format(
          "set app /app.name:\"{0}\" -applicationPool:\"{1}\"",
          fullWebAppName,
          appPoolName);

      RunAppCmd(machineName, appCmdCommand);
    }

    public string GetWebApplicationPath(string machineName, string fullWebAppName)
    {
      if (string.IsNullOrEmpty(machineName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "machineName");
      }

      if (string.IsNullOrEmpty(fullWebAppName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "fullWebAppName");
      }

      var msDeployArgs =
        new[]
          {
            "-verb:dump",
            string.Format("-source:appHostConfig=\"{0}\",computerName=\"{1}\"",  fullWebAppName, machineName),
            "-xml"
          };

      try
      {
        string consoleOutput;
        _msDeploy.Run(msDeployArgs, out consoleOutput);
        return XDocument.Parse(consoleOutput).Descendants("virtualDirectory").Single().Descendants("dirPath").Single().Attribute("path").Value;
      }
      catch (MsDeployException e)
      {
        var pat = new Regex(@"(Application '[^']+' does not exist in site '[^']+'\.)|(Site '[^']+' does not exist\.)");
        if (pat.IsMatch(e.ConsoleError))
        {
          return null;
        }
        throw;
      }
    }

    #endregion

    #region Private helper methods

    private static IisAppPoolInfo CreateIisAppPoolInfoFromMatch(Match match)
    {
      string appPoolName = match.Groups["AppPoolName"].Value;
      string appPoolVersionStr = match.Groups["AppPoolVersion"].Value;
      string appPoolModeStr = match.Groups["AppPoolMode"].Value;
      IisAppPoolVersion appPoolVersion;
      IisAppPoolMode appPoolMode;

      appPoolVersionStr = appPoolVersionStr.Replace(".", "_");

      if (!Enum.TryParse(appPoolVersionStr, true, out appPoolVersion))
      {
        throw new InternalException(string.Format("Couldn't parse application pool version: '{0}'.", appPoolVersionStr));
      }

      if (!Enum.TryParse(appPoolModeStr, true, out appPoolMode))
      {
        throw new InternalException(string.Format("Couldn't parse application pool mode: '{0}'.", appPoolModeStr));
      }

      return new IisAppPoolInfo(appPoolName, appPoolVersion, appPoolMode);
    }


    private static void HandleAppCmdExitCode(string exitCodeString, string machineName)
    {
      if (exitCodeString == "0xB7") // app pool already exists
      {
        throw new InvalidOperationException(string.Format("Error while running appcmd.exe. Can't create an application pool because it already exists on the target machine."));
      }

      if (exitCodeString == "0x32") // site doesn't exist
      {
        throw new InvalidOperationException(string.Format("Error while running appcmd.exe. Can't set an app's application pool because the app doesn't exist on the target machine."));
      }

      if (exitCodeString != "0x0") // generic error
      {
        throw new InternalException(string.Format("Error while running appcmd.exe. Make sure that the file '{0}' is present on the target machine ('{1}').", _RemoteAppCmdExePath, machineName));
      }
    }

    private IDictionary<string, IisAppPoolInfo> GetAppPoolsImpl(string machineName, int retriesCount, int sleepBeforeRetryInMs)
    {
      /*
      ArturS: Imho we should be using this: http://stackoverflow.com/questions/4791701/create-an-application-pool-that-uses-net-4-0 (reply #2)
      (Microsoft.Web.Administration)
      */

      string consoleOutput = RunAppCmd(machineName, "list apppool");

      MatchCollection appPoolInfosMatches = _AppPoolInfoRegex.Matches(consoleOutput);

      Dictionary<string, IisAppPoolInfo> appPools =
        (from Match appPoolInfoMatch in appPoolInfosMatches select appPoolInfoMatch)
          .ToDictionary(
            match => match.Groups["AppPoolName"].Value,
            match => CreateIisAppPoolInfoFromMatch(match));

      // sometimes msdeploy doesn't get the output from appcmd.exe,
      // so we think there are no application pools at all;
      // let's retry the operation just to be sure
      if (appPools.Count == 0 && retriesCount > 0)
      {
        // TODO IMM HI: log
        Console.WriteLine("Will retry.");

        if (sleepBeforeRetryInMs > 0)
        {
          // TODO IMM HI: log
          Console.WriteLine("Sleeping for {0:F2} s.", sleepBeforeRetryInMs / 1000.0);
          Thread.Sleep(sleepBeforeRetryInMs);
        }
        else
        {
          // we'll start with 100 ms (2 * 50 ms)
          sleepBeforeRetryInMs = 50;
        }

        return GetAppPoolsImpl(machineName, retriesCount - 1, 2 * sleepBeforeRetryInMs);
      }

      return appPools;
    }

    private string RunAppCmd(string machineName, string appCmdArgs)
    {
      var msDeployArgs =
        new[]
          {
            "-verb:sync",
            string.Format("-source:runCommand=\"{0} {1}\"", _RemoteAppCmdExePath, appCmdArgs),
            string.Format("-dest:auto,computerName=\"{0}\"", machineName),
          };

      string consoleOutput;

      _msDeploy.Run(msDeployArgs, out consoleOutput);

      Match exitCodeMatch = _ExitCodeRegex.Match(consoleOutput);
      string exitCodeString = "?";

      if (exitCodeMatch.Success)
      {
        exitCodeString = exitCodeMatch.Groups["ExitCode"].Value;
      }

      HandleAppCmdExitCode(exitCodeString, machineName);

      return consoleOutput;
    }

    #endregion
  }
}