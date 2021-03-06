using UberDeployer.Common.IO;
using UberDeployer.Core.Configuration;
using UberDeployer.Core.Deployment;
using UberDeployer.Core.Deployment.Pipeline;
using UberDeployer.Core.Deployment.Pipeline.Modules;
using UberDeployer.Core.Deployment.Tasks;
using UberDeployer.Core.Deployment.Steps;
using UberDeployer.Core.Domain;
using UberDeployer.Core.ExternalDataCollectors.DependentProjectsSelection;
using UberDeployer.Core.Management.Db;
using UberDeployer.Core.Management.Db.DbManager;
using UberDeployer.Core.Management.FailoverCluster;
using UberDeployer.Core.Management.Iis;
using UberDeployer.Core.Management.Metadata;
using UberDeployer.Core.Management.MsDeploy;
using UberDeployer.Core.Management.NtServices;
using UberDeployer.Core.Management.ScheduledTasks;
using UberDeployer.Core.TeamCity;

namespace UberDeployer.Core
{
  // TODO IMM HI: rethink
  public interface IObjectFactory
  {
    IApplicationConfiguration CreateApplicationConfiguration();

    IProjectInfoRepository CreateProjectInfoRepository();

    IEnvironmentInfoRepository CreateEnvironmentInfoRepository();

    IArtifactsRepository CreateArtifactsRepository();

    IDeploymentRequestRepository CreateDeploymentRequestRepository();

    INtServiceManager CreateNtServiceManager();

    IIisManager CreateIIisManager();

    ITaskScheduler CreateTaskScheduler();

    IMsDeploy CreateIMsDeploy();

    IDeploymentPipeline CreateDeploymentPipeline();

    IPasswordCollector CreatePasswordCollector();

    IDbScriptRunnerFactory CreateDbScriptRunnerFactory();

    IDbVersionProvider CreateDbVersionProvider();

    IFailoverClusterManager CreateFailoverClusterManager();

    IDirectoryAdapter CreateDirectoryAdapter();

    IProjectMetadataExplorer CreateProjectMetadataExplorer();

    IDirPathParamsResolver CreateDirPathParamsResolver();

    IFileAdapter CreateFileAdapter();

    IZipFileAdapter CreateZipFileAdapter();
    
    IEnvironmentDeployInfoRepository CreateEnvironmentDeployInfoRepository();

    IDbManagerFactory CreateDbManagerFactory();

    IScriptsToRunSelector CreateScriptsToRunWebSelector();

    IScriptsToRunSelector CreateScriptsToRunWebSelectorForEnvironmentDeploy();

    ITeamCityRestClient CreateTeamCityRestClient();

    IMsSqlDatabasePublisher CreateMsSqlDatabasePublisher();

    IEnvDeploymentPipeline CrateEnvDeploymentPipeline();

    IDependentProjectsToDeployWebSelector CreateDependentProjectsToDeployWebSelector();

    IUserNameNormalizer CreateUserNameNormalizer();
  }
}
