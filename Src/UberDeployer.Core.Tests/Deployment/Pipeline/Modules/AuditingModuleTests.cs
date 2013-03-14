﻿using System;
using Moq;
using NUnit.Framework;
using UberDeployer.Core.Deployment;
using UberDeployer.Core.Deployment.Pipeline;
using UberDeployer.Core.Deployment.Pipeline.Modules;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Tests.Generators;

namespace UberDeployer.Core.Tests.Deployment.Pipeline.Modules
{
  [TestFixture]
  public class AuditingModuleTests
  {
    [Test]
    public void Constructor_WhenDeploymentRequestRepositoryIsNull_ThrowsArgumentNullException()
    {
      Assert.Throws<ArgumentNullException>(() => new AuditingModule(null));
    }

    [Test]
    public void OnDeploymentTaskStarting_DoNothing_SoDoesNotThrow()
    {
      var deploymentRequestRepository = new Mock<IDeploymentRequestRepository>();
      var auditingModule = new AuditingModule(deploymentRequestRepository.Object);

      var environmentInfoRepository = new Mock<IEnvironmentInfoRepository>();
      var artifactsRepository = new Mock<IArtifactsRepository>();
      var deploymentTask = new DeployTerminalAppDeploymentTask(environmentInfoRepository.Object, artifactsRepository.Object);
      var deploymentContext = new DeploymentContext("requester");
      DeploymentInfo deploymentInfo = DeploymentInfoGenerator.GetTerminalAppDeploymentInfo();

      Assert.DoesNotThrow(() => auditingModule.OnDeploymentTaskStarting(deploymentInfo, deploymentTask, deploymentContext));
    }

    [Test]
    public void OnDeploymentTaskFinished_ExpectAddDeploymnetRequest()
    {
      DeploymentInfo deploymentInfo = DeploymentInfoGenerator.GetTerminalAppDeploymentInfo();

      var deploymentRequestRepository = new Mock<IDeploymentRequestRepository>(MockBehavior.Strict);
      var auditingModule = new AuditingModule(deploymentRequestRepository.Object);

      var environmentInfoRepository = new Mock<IEnvironmentInfoRepository>();
      var artifactsRepository = new Mock<IArtifactsRepository>();

      var deploymentTask = new DeployTerminalAppDeploymentTask(environmentInfoRepository.Object, artifactsRepository.Object);
      var deploymentContext = new DeploymentContext("requester");

      deploymentRequestRepository
        .Setup(
          drr =>
          drr.AddDeploymentRequest(
            It.Is<DeploymentRequest>(
              r => r.ProjectName == deploymentInfo.ProjectName
                && r.TargetEnvironmentName == deploymentInfo.TargetEnvironmentName)));

      environmentInfoRepository
        .Setup(x => x.GetByName(deploymentInfo.TargetEnvironmentName))
        .Returns(DeploymentDataGenerator.GetEnvironmentInfo());

      deploymentTask.Prepare(deploymentInfo);

      Assert.DoesNotThrow(() => auditingModule.OnDeploymentTaskFinished(deploymentInfo, deploymentTask, deploymentContext));
    }
  }
}
