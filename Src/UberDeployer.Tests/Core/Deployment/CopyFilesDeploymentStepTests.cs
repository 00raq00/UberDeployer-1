﻿using System;
using System.IO;

using NUnit.Framework;

using UberDeployer.Common.IO;
using UberDeployer.Core.Deployment.Steps;
using UberDeployer.Core.Deployment.Tasks;

namespace UberDeployer.Tests.Core.Deployment
{
  [TestFixture]
  public class CopyFilesDeploymentStepTests
  {
    private DirectoryAdapter _directoryAdapter;
    private FileAdapter _fileAdapter;

    [SetUp]
    public void SetUp()
    {
      _directoryAdapter = new DirectoryAdapter();
      _fileAdapter = new FileAdapter();
    }

    [Test]
    public void Test_copying_all_files()
    {
      const string srcDirPath = "Core\\TestData\\TestSrcDir";
      const string dstDirPath = "Core\\TestData\\TestDstDir";

      try
      {
        var copyFilesDeploymentStep =
          new CopyFilesDeploymentStep(
            _directoryAdapter, 
            new Lazy<string>(() => srcDirPath), 
            new Lazy<string>(() => dstDirPath));

        copyFilesDeploymentStep.PrepareAndExecute();

        Assert.IsTrue(Directory.Exists(dstDirPath));

        Assert.AreEqual(
          Directory.GetFiles(srcDirPath, "*.*", SearchOption.AllDirectories).Length,
          Directory.GetFiles(dstDirPath, "*.*", SearchOption.AllDirectories).Length);
      }
      finally
      {
        if (Directory.Exists(dstDirPath))
        {
          Directory.Delete(dstDirPath, true);
        }
      }
    }

    [Test]
    public void Test_copying_all_files_when_dst_exists()
    {
      const string srcDirPath = "Core\\TestData\\TestSrcDir";
      const string dstDirPath = "Core\\TestData\\TestDstDir";

      try
      {
        Directory.CreateDirectory(dstDirPath);

        var copyFilesDeploymentStep =
          new CopyFilesDeploymentStep(
            _directoryAdapter, 
            new Lazy<string>(() => srcDirPath),
            new Lazy<string>(() => dstDirPath));

        copyFilesDeploymentStep.PrepareAndExecute();

        Assert.IsTrue(Directory.Exists(dstDirPath));

        Assert.AreEqual(
          Directory.GetFiles(srcDirPath, "*.*", SearchOption.AllDirectories).Length,
          Directory.GetFiles(dstDirPath, "*.*", SearchOption.AllDirectories).Length);
      }
      finally
      {
        if (Directory.Exists(dstDirPath))
        {
          Directory.Delete(dstDirPath, true);
        }
      }
    }

    [Test]
    public void Test_copying_all_files_throws_when_no_src()
    {
      const string srcDirPath = "Core\\TestData\\aoisdiasyd";
      const string dstDirPath = "Core\\TestData\\TestDstDir";

      try
      {
        var copyFilesDeploymentStep =
          new CopyFilesDeploymentStep(
            _directoryAdapter, 
            new Lazy<string>(() => srcDirPath),
            new Lazy<string>(() => dstDirPath));

        Assert.Throws<DeploymentTaskException>(() => copyFilesDeploymentStep.PrepareAndExecute());
      }
      finally
      {
        if (Directory.Exists(dstDirPath))
        {
          Directory.Delete(dstDirPath, true);
        }
      }
    }
  }
}
