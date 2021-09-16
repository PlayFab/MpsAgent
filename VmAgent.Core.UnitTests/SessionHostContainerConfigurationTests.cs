using Microsoft.Azure.Gaming.VmAgent.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VmAgentCoreInterface = Microsoft.Azure.Gaming.VmAgent.Core.Interfaces;
using Docker.DotNet;
using Microsoft.Azure.Gaming.AgentInterfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.Azure.Gaming.LocalMultiplayerAgent.Config;
using System;
using Microsoft.Azure.Gaming.VmAgent.Core.Interfaces;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class SessionHostContainerConfigurationUnitTests
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
        
        private static Guid TestTitleIdUlong = Guid.NewGuid();
        private static Guid TestBuildId = Guid.NewGuid();
        private static Guid TestRegion = Guid.NewGuid();
        private static string TestLogFolderId = "TestLogFolderId";
        private static string TestPublicIpV4Address = "127.0.0.1";

        private DirectoryInfo _subDirectoryInfo;
        private DirectoryInfo _directoryInfo;
        private VmConfiguration _vmConfiguration;
        private MultiLogger _logger;
        private Mock<VmAgentCoreInterface.ISystemOperations> _systemOperations;
        private Mock<VmAgentCoreInterface.ISessionHostManager> _sessionHostManager;
        private Mock<IDockerClient> _dockerClient;
        private SessionHostsStartInfo _sessionHostsStartInfo;

        [TestInitialize]
        public void BeforeEachTest()
        {
            _directoryPath = Path.Combine(_root, _directoryName);
            _directoryInfo = new DirectoryInfo(_directoryPath);
            _subdirectoryPath = Path.Combine(_directoryPath, _subdirectoryName);
            _subDirectoryInfo = new DirectoryInfo(_subdirectoryPath);
            _directoryFilePath = Path.Combine(_directoryPath, _directoryFileName);
            FileInfo directoryFileInfo = new FileInfo(_directoryFilePath);
            _subdirectoryFilePath = Path.Combine(_subdirectoryPath, _subdirectoryFileName);
            FileInfo subDirectoryFileInfo = new FileInfo(_subdirectoryFilePath);
            //_defaultSourceFile = Path.Combine(_root, "Source");
            string AssignmentId = $"{TestTitleIdUlong}:{TestBuildId}:{TestRegion}";

            _logger = new MultiLogger(NullLogger.Instance, new TelemetryClient(TelemetryConfiguration.CreateDefault()));
            _systemOperations = new Mock<Microsoft.Azure.Gaming.VmAgent.Core.Interfaces.ISystemOperations>();
            _dockerClient = new Mock<IDockerClient>();
            _sessionHostManager = new Mock<ISessionHostManager>();
            _vmConfiguration = new VmConfiguration(56001, "vmid", new VmDirectories("root"), true);
            
            _sessionHostsStartInfo = new SessionHostsStartInfo();
            _sessionHostsStartInfo.AssignmentId = AssignmentId;

            _sessionHostManager.Setup(x => x.VmAgentSettings).Returns(new VmAgentSettings() { EnableCrashDumpProcessing = false });

        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestEnvVariablesWithSessionProcessOnWindow()
        {
            _sessionHostsStartInfo.SessionHostType = SessionHostType.Process;
            _sessionHostsStartInfo.PublicIpV4Address = TestPublicIpV4Address;

            SessionHostContainerConfiguration sessionHostContainerConfiguration =
                new SessionHostContainerConfiguration(_vmConfiguration, _logger, _systemOperations.Object, _dockerClient.Object, _sessionHostsStartInfo, _sessionHostManager.Object);
            IDictionary<string, string> envVariables = 
                sessionHostContainerConfiguration.GetEnvironmentVariablesForSessionHost(0, TestLogFolderId, _sessionHostManager.Object.VmAgentSettings);
            string log = envVariables["PF_SERVER_LOG_DIRECTORY"];
            
            Assert.IsFalse(log == "");
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestEnvVariablesWithSessionContainerOnWindow()
        {
            _sessionHostsStartInfo.SessionHostType = SessionHostType.Container;

            SessionHostContainerConfiguration sessionHostContainerConfiguration =
                new SessionHostContainerConfiguration(_vmConfiguration, _logger, _systemOperations.Object, _dockerClient.Object, _sessionHostsStartInfo, _sessionHostManager.Object);
            IDictionary<string, string> envVariables =
                sessionHostContainerConfiguration.GetEnvironmentVariablesForSessionHost(0, "LogFolderId", _sessionHostManager.Object.VmAgentSettings);
            string log = envVariables["PF_SERVER_LOG_DIRECTORY"];

            Assert.IsFalse(log == "");
        }

        public void validateEnvironmentVariablesForSessionHost(IDictionary<string, string> envVariables, SessionHostType HostType)
        {

        }
    }
}