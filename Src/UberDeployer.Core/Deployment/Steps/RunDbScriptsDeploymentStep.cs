using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Deployment.Tasks;
using UberDeployer.Core.Management.Db;

namespace UberDeployer.Core.Deployment.Steps
{
  public class RunDbScriptsDeploymentStep : DeploymentStep
  {
    private static readonly string[] _versionInsertNotRequiredScriptTails = { "notrans", "migration" };

    private readonly string _databaseServerMachineName;

    private readonly IEnumerable<DbScriptToRun> _scriptPathsToRunEnumerable;

    private readonly IDbScriptRunner _dbScriptRunner;

    public RunDbScriptsDeploymentStep(
      IDbScriptRunner dbScriptRunner, string databaseServerMachineName, IEnumerable<DbScriptToRun> scriptPathsToRunEnumerable)
    {
      Guard.NotNull(dbScriptRunner, "dbScriptRunner");
      Guard.NotNullNorEmpty(databaseServerMachineName, "databaseServerMachineName");

      if (scriptPathsToRunEnumerable == null)
      {
        throw new ArgumentNullException("scriptPathsToRunEnumerable");
      }

      _databaseServerMachineName = databaseServerMachineName;
      _scriptPathsToRunEnumerable = scriptPathsToRunEnumerable;
      _dbScriptRunner = dbScriptRunner;
    }

    protected override void DoExecute()
    {
      string executedScriptName = null;

      try
      {
        List<DbScriptToRun> scriptPathsToRunList = _scriptPathsToRunEnumerable.ToList();

        if (scriptPathsToRunList.Count <= 0)
        {
          PostDiagnosticMessage("There are no scripts to run.", DiagnosticMessageType.Info);
        }
        else
        {
          PostDiagnosticMessage(
            string.Format("Will run '{0}' scripts.", scriptPathsToRunList.Count), DiagnosticMessageType.Info);

          foreach (DbScriptToRun scriptPathToRun in scriptPathsToRunList)
          {
            executedScriptName = Path.GetFileName(scriptPathToRun.ScriptPath);

            using (var sr = new StreamReader(scriptPathToRun.ScriptPath))
            {
              string script = sr.ReadToEnd();

              if (ShouldBeVersionInsertPresent(executedScriptName) && !DbScriptToRun.IsVersionInsertPresent(scriptPathToRun.DbVersion, script))
              {
                throw new DeploymentTaskException(string.Format("Script {0} that should be run does not have necessary version insert.", scriptPathToRun.DbVersion));
              }

              _dbScriptRunner.Execute(script);

              PostDiagnosticMessage("Script executed successfully: " + executedScriptName, DiagnosticMessageType.Info);
            }
          }
        }
      }
      catch (DbScriptRunnerException exc)
      {
        string message = string.Format("Script execution failed, script name: '{0}'. {1}.", executedScriptName, exc.Message);

        Exception eLocal = exc.InnerException;

        while (eLocal != null)
        {
          PostDiagnosticMessage("Inner exception: " + eLocal.Message, DiagnosticMessageType.Error);

          eLocal = eLocal.InnerException;
        }

        throw new DeploymentTaskException(message, exc);
      }
    }

    private bool ShouldBeVersionInsertPresent(string scriptFileName)
    {
      string fileName = Path.GetFileNameWithoutExtension(scriptFileName);

      if (string.IsNullOrEmpty(fileName))
      {
        throw new DeploymentTaskException(string.Format("Invalid script file name: '{0}'", scriptFileName));
      }

      foreach (string versionInsertNotRequiredScriptTail in _versionInsertNotRequiredScriptTails)
      {
        if (fileName.EndsWith(versionInsertNotRequiredScriptTail, StringComparison.OrdinalIgnoreCase))
        {
          return false;
        }
      }

      return true;
    }

    public override string Description
    {
      get
      {
        return string.Format("Run db scripts on server '{0}'.", _databaseServerMachineName);
      }
    }
  }
}