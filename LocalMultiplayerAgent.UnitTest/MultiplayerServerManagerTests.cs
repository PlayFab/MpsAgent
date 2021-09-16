// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.UnitTests
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using AgentInterfaces;
    using Core.Interfaces;
    using FluentAssertions;
    using global::VmAgent.Core.Interfaces;
    using LocalMultiplayerAgent.Config;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Azure.Gaming.VmAgent.Core;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MultiplayerServerManagerTests
    {
        private string _root = "C:\\";
        private string _directoryPath;
        private string _subdirectoryPath;
        private string _directoryFilePath;
        private string _subdirectoryFilePath;
        private string _subdirectoryName = "subdirectory";
        private string _directoryName = "directory";
        private string _directoryFileName = "file";
        private string _subdirectoryFileName = "subfile";
        private string _defaultFileContent = "This is not the file you are looking for";

        private MultiLogger _logger;
        private VmConfiguration _vmConfiguration;
        private Mock<ISystemOperations> _mockSystemOperations;
        private Mock<ISessionHostRunnerFactory> _sessionHostRunnerFactory;
        private SystemOperations _systemOperations;
        private Mock<IFileSystemOperations> _mockFileSystemOperations;

        [TestInitialize]
        public void BeforeEachTest()
        {
            _sessionHostRunnerFactory = new Mock<ISessionHostRunnerFactory>();
            _mockFileSystemOperations = new Mock<IFileSystemOperations>();
            _mockSystemOperations = new Mock<ISystemOperations>();
            _vmConfiguration = new VmConfiguration(56001, "vmid", new VmDirectories("root"), true);
            _logger = new MultiLogger(NullLogger.Instance, new TelemetryClient(TelemetryConfiguration.CreateDefault()));
            _systemOperations = new SystemOperations(_vmConfiguration, _logger, _mockFileSystemOperations.Object);
        
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void logFolderVerificationTest()
        {
            string logFolder = Path.Combine(_vmConfiguration.VmDirectories.GameLogsRootFolderVm, new Guid().ToString());
            //string rootOutputFolder = Path.Combine(settings.OutputFolder, "PlayFabVmAgentOutput", DateTime.Now.ToString("s").Replace(':', '-'));
            //Console.WriteLine($"Root output folder: {rootOutputFolder}");
        }
      
    }
}
