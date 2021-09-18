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
using System;
using Microsoft.Azure.Gaming.VmAgent.Core.Interfaces;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class SessionHostContainerConfigurationUnitTests
    {
        // Directory Variables
        private string _root = "C:\\";
        private string _VmDirectoryRoot = "root";
        private string _VmDirectoryContainerRoot = "/data";
    
        // Environment Variables
        private const string TitleIdEnvVariable = "PF_TITLE_ID";
        private const string BuildIdEnvVariable = "PF_BUILD_ID";
        private const string RegionEnvVariable = "PF_REGION";
        private const string LogsDirectoryEnvVariable = "PF_SERVER_LOG_DIRECTORY";
        private const string ServerInstanceNumberEnvVariable = "PF_SERVER_INSTANCE_NUMBER";

        private static Guid TestTitleIdUlong = Guid.NewGuid();
        private static Guid TestBuildId = Guid.NewGuid();
        private static Guid TestRegion = Guid.NewGuid();
        private static string TestLogFolderId = "TestLogFolderId";
        private static string TestPublicIpV4Address = "127.0.0.1";

        private VmConfiguration _vmConfiguration;
        private MultiLogger _logger;
        private Mock<VmAgentCoreInterface.ISystemOperations> _systemOperations;
        private Mock<ISessionHostManager> _sessionHostManager;
        private Mock<IDockerClient> _dockerClient;
        private SessionHostsStartInfo _sessionHostsStartInfo;

        [TestInitialize]
        public void BeforeEachTest()
        {
            string AssignmentId = $"{TestTitleIdUlong}:{TestBuildId}:{TestRegion}";

            _logger = new MultiLogger(NullLogger.Instance, new TelemetryClient(TelemetryConfiguration.CreateDefault()));
            _systemOperations = new Mock<Microsoft.Azure.Gaming.VmAgent.Core.Interfaces.ISystemOperations>();
            _dockerClient = new Mock<IDockerClient>();
            _sessionHostManager = new Mock<ISessionHostManager>();
            _vmConfiguration = new VmConfiguration(56001, "vmid", new VmDirectories(_VmDirectoryRoot), true);
            //string rootOutputFolder = Path.Combine(settings.OutputFolder, "PlayFabVmAgentOutput", DateTime.Now.ToString("s").Replace(':', '-'));

            _sessionHostsStartInfo = new SessionHostsStartInfo();
            _sessionHostsStartInfo.AssignmentId = AssignmentId;
            _sessionHostsStartInfo.PublicIpV4Address = TestPublicIpV4Address;

            _sessionHostManager.Setup(x => x.VmAgentSettings).Returns(new VmAgentSettings() { EnableCrashDumpProcessing = false });
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestEnvVariablesWithSessionProcessOnWindow()
        {
            _sessionHostsStartInfo.SessionHostType = SessionHostType.Process;

            SessionHostProcessConfiguration sessionHostProcessConfiguration =
                new SessionHostProcessConfiguration(_vmConfiguration, _logger, _systemOperations.Object, _sessionHostsStartInfo);
            IDictionary<string, string> envVariables =
                sessionHostProcessConfiguration.GetEnvironmentVariablesForSessionHost(0, TestLogFolderId, _sessionHostManager.Object.VmAgentSettings);

            Assert.AreEqual(envVariables[LogsDirectoryEnvVariable], Path.Combine(_VmDirectoryRoot, "GameLogs", TestLogFolderId));
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestEnvVariablesWithSessionContainerOnWindow()
        {
            _sessionHostManager.Setup(x => x.LinuxContainersOnWindows).Returns(true);

            VmPathHelper.AdaptFolderPathsForLinuxContainersOnWindows(_vmConfiguration);

            _sessionHostsStartInfo.SessionHostType = SessionHostType.Container;

            SessionHostContainerConfiguration sessionHostContainerConfiguration =
                new SessionHostContainerConfiguration(_vmConfiguration, _logger, _systemOperations.Object, _dockerClient.Object, _sessionHostsStartInfo, _sessionHostManager.Object);

            IDictionary<string, string> envVariables =
                sessionHostContainerConfiguration.GetEnvironmentVariablesForSessionHost(0, TestLogFolderId, _sessionHostManager.Object.VmAgentSettings);

            string vmContainerPath = _VmDirectoryContainerRoot + "/GameLogs" + "/";
            Assert.AreEqual(envVariables[LogsDirectoryEnvVariable], vmContainerPath);
        }
    }
}