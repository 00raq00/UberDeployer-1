using System;
using System.Collections.Generic;
using System.ServiceModel;
using UberDeployer.Agent.Proxy.Dto;
using UberDeployer.Agent.Proxy.Dto.EnvDeployment;
using UberDeployer.Agent.Proxy.Dto.Metadata;
using UberDeployer.Agent.Proxy.Dto.TeamCity;
using UberDeployer.Agent.Proxy.Faults;

namespace UberDeployer.Agent.Proxy
{
  [ServiceContract]
  public interface IAgentService
  {
    [OperationContract]
    [FaultContract(typeof(ProjectNotFoundFault))]
    void Deploy(Guid deploymentId, Guid uniqueClientId, string requesterIdentity, DeploymentInfo deploymentInfo);

    [OperationContract]
    [FaultContract(typeof(ProjectNotFoundFault))]
    void DeployAsync(Guid deploymentId, Guid uniqueClientId, string requesterIdentity, DeploymentInfo deploymentInfoDto);

    [OperationContract]
    [FaultContract(typeof(ProjectNotFoundFault))]
    void CreatePackageAsync(Guid deploymentId, Guid uniqueClientId, string requesterIdentity, DeploymentInfo deploymentInfo, string packageDirPath);

    [OperationContract]
    List<ProjectInfo> GetProjectInfos(ProjectFilter projectFilter);

    [OperationContract]
    List<EnvironmentInfo> GetEnvironmentInfos();

    [OperationContract]
    [FaultContract(typeof(EnvironmentNotFoundFault))]
    List<string> GetWebMachineNames(string environmentName);

    [OperationContract]
    [FaultContract(typeof(ProjectNotFoundFault))]
    List<ProjectConfiguration> GetProjectConfigurations(string projectName);

    [OperationContract]
    [FaultContract(typeof(ProjectNotFoundFault))]
    [FaultContract(typeof(ProjectConfigurationNotFoundFault))]
    List<ProjectConfigurationBuild> GetProjectConfigurationBuilds(string projectName, string projectConfigurationName, string branchName, int maxCount, bool onlyPinned);

    [OperationContract]
    [FaultContract(typeof(ProjectNotFoundFault))]
    [FaultContract(typeof(EnvironmentNotFoundFault))]
    List<string> GetWebAppProjectTargetUrls(string projectName, string environmentName);

    // TODO IMM HI: separate interface?
    [OperationContract]
    List<DeploymentRequest> GetDeploymentRequests(int startIndex, int maxCount);

    // TODO IMM HI: separate interface?
    [OperationContract]
    List<DiagnosticMessage> GetDiagnosticMessages(Guid uniqueClientId, long lastSeenMaxMessageId, DiagnosticMessageType minMessageType);

    // TODO IMM HI: separate interface?
    [OperationContract]
    ProjectMetadata GetProjectMetadata(string projectName, string environmentName);

    [OperationContract]
    void SetCollectedCredentialsForAsynchronousWebCredentialsCollector(Guid deploymentId, string password);

    [OperationContract]
    void SetSelectedDbScriptsToRun(Guid deploymentId, DbScriptsToRunSelection scriptsToRunSelection);

    [OperationContract]
    void CancelDbScriptsSelection(Guid deploymentId);

    [OperationContract]
    [FaultContract(typeof(ProjectNotFoundFault))]
    [FaultContract(typeof(EnvironmentNotFoundFault))]
    string GetDefaultPackageDirPath(string environmentName, string projectName);

    [OperationContract]
    [FaultContract(typeof(EnvironmentDeployConfigurationNotFoundFault))]
    List<string> GetProjectsForEnvironmentDeploy(string environmentName);

    [OperationContract]
    [FaultContract(typeof(EnvironmentDeployConfigurationNotFoundFault))]
    [FaultContract(typeof(EnvironmentNotFoundFault))]
    void DeployEnvironmentAsync(Guid uniqueClientId, string requesterIdentity, string targetEnvironment, List<ProjectToDeploy> projects);

    [OperationContract]
    void SetSelectedDependentProjectsToDeploy(Guid deploymentId, List<DependentProject> dependenciesToDeploy);

    [OperationContract]
    void SkipDependentProjectsSelection(Guid deploymentId);
    
    [OperationContract]
    void CancelDependentProjectsSelection(Guid deploymentId);
  }
}
  