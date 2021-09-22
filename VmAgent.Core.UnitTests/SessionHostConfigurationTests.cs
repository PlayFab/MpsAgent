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
using FluentAssertions;
using Microsoft.Azure.Gaming.VmAgent.Model;
using Newtonsoft.Json;
using System.Linq;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class SessionHostContainerConfigurationUnitTests
    {
        // Directory Variables
        private string _root = "C:\\";
        private string _VmDirectoryRoot = "root";
        private string _VmDirectoryContainerRoot = "/data";
    
        // Environment Variables in VM 
        private const string LogsDirectoryEnvVariable = "PF_SERVER_LOG_DIRECTORY";
        private const string SharedContentFolderEnvVariable = "PF_SHARED_CONTENT_FOLDER";
        private const string ConfigFileEnvVariable = "GSDK_CONFIG_FILE";
        private const string CertificateFolderEnvVariable = "CERTIFICATE_FOLDER";

        private static Guid TestTitleIdUlong = Guid.NewGuid();
        private static Guid TestBuildId = Guid.NewGuid();
        private static Guid TestRegion = Guid.NewGuid();
        private static string TestLogFolderId = "TestLogFolderId";
        private static string TestPublicIpV4Address = "127.0.0.1";
        private static string TestdDockerId = "dockerId";
        private static string TestAgentIPaddress = "host.docker.internal";
        private static string TestVmId = "vmid";


        private VmConfiguration _vmConfiguration;
        private MultiLogger _logger;
        //private Mock<VmAgentCoreInterface.ISystemOperations> _systemOperations;
        private SystemOperations _systemOperations;
        private Mock<ISessionHostManager> _sessionHostManager;
        private Mock<IDockerClient> _dockerClient;
        private SessionHostsStartInfo _sessionHostsStartInfo;

        [TestInitialize]
        public void BeforeEachTest()
        {
            string AssignmentId = $"{TestTitleIdUlong}:{TestBuildId}:{TestRegion}";

            _logger = new MultiLogger(NullLogger.Instance, new TelemetryClient(TelemetryConfiguration.CreateDefault()));
            _systemOperations = new SystemOperations();
            _dockerClient = new Mock<IDockerClient>();
            _sessionHostManager = new Mock<ISessionHostManager>();
            _vmConfiguration = new VmConfiguration(56001, TestVmId, new VmDirectories(_VmDirectoryRoot), true);
          
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
                new SessionHostProcessConfiguration(_vmConfiguration, _logger, _systemOperations, _sessionHostsStartInfo);
            IDictionary<string, string> envVariables =
                sessionHostProcessConfiguration.GetEnvironmentVariablesForSessionHost(0, TestLogFolderId, _sessionHostManager.Object.VmAgentSettings);

            Assert.AreEqual(envVariables[LogsDirectoryEnvVariable], Path.Combine(_VmDirectoryRoot, "GameLogs", TestLogFolderId));
            Assert.AreEqual(envVariables[SharedContentFolderEnvVariable], Path.Combine(_VmDirectoryRoot, "GameSharedContent"));
            Assert.AreEqual(envVariables[ConfigFileEnvVariable], Path.Combine(_VmDirectoryRoot, "Config", "SH0", "gsdkConfig.json"));
            Assert.AreEqual(envVariables[CertificateFolderEnvVariable], Path.Combine(_VmDirectoryRoot, "GameCertificates"));
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestEnvVariablesWithSessionContainerOnWindow()
        {
            _sessionHostManager.Setup(x => x.LinuxContainersOnWindows).Returns(true);

            VmPathHelper.AdaptFolderPathsForLinuxContainersOnWindows(_vmConfiguration);

            _sessionHostsStartInfo.SessionHostType = SessionHostType.Container;

            SessionHostContainerConfiguration sessionHostContainerConfiguration =
                new SessionHostContainerConfiguration(_vmConfiguration, _logger, _systemOperations, _dockerClient.Object, _sessionHostsStartInfo, _sessionHostManager.Object);

            IDictionary<string, string> envVariables =
                sessionHostContainerConfiguration.GetEnvironmentVariablesForSessionHost(0, TestLogFolderId, _sessionHostManager.Object.VmAgentSettings);

            string containerPath = _VmDirectoryContainerRoot + "/GameLogs" + "/";
            string sharedContentFolderPath = _VmDirectoryContainerRoot + "/GameSharedContent";
            string gsdkConfigFilePath = _VmDirectoryContainerRoot + "/Config" + "/gsdkConfig.json";
            string certificateFolderPath = _VmDirectoryContainerRoot + "/GameCertificates";
            
            Assert.AreEqual(envVariables[LogsDirectoryEnvVariable], containerPath);
            Assert.AreEqual(envVariables[SharedContentFolderEnvVariable], sharedContentFolderPath);
            Assert.AreEqual(envVariables[ConfigFileEnvVariable], gsdkConfigFilePath);
            Assert.AreEqual(envVariables[CertificateFolderEnvVariable], certificateFolderPath);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestCreateLegacyGSDKConfigFileWithSessionContainer()
        {
            _vmConfiguration = new VmConfiguration(56001, TestVmId, new VmDirectories(_root), true);

            _sessionHostManager.Setup(x => x.LinuxContainersOnWindows).Returns(true);

            VmPathHelper.AdaptFolderPathsForLinuxContainersOnWindows(_vmConfiguration);

            _sessionHostsStartInfo.SessionHostType = SessionHostType.Container;
            _sessionHostsStartInfo.PortMappingsList = new List<List<PortMapping>>() {
                new List<PortMapping>()
                {
                    new PortMapping()
                    {
                        GamePort= new Port() {
                            Name="port",
                            Number=80,
                            Protocol= "TCP"
                        },
                        PublicPort= 1234,
                        NodePort = 56001
                    }
                }
            };

            List<PortMapping> mockPortmapping = _sessionHostsStartInfo.PortMappingsList[0];

            SessionHostContainerConfiguration sessionHostContainerConfiguration =
                new SessionHostContainerConfiguration(_vmConfiguration, _logger, _systemOperations, _dockerClient.Object, _sessionHostsStartInfo, _sessionHostManager.Object);

            sessionHostContainerConfiguration.Create(0, TestdDockerId, TestAgentIPaddress, _vmConfiguration, TestLogFolderId);

            string gsdkConfigFilePath = Path.Combine(_root, "Config", "SH0", "gsdkConfig.json");

            GsdkConfiguration gsdkConfigExpected = new GsdkConfiguration()
            {
                HeartbeatEndpoint = $"{TestAgentIPaddress}:56001",
                SessionHostId = TestdDockerId,
                VmId = TestVmId,
                LogFolder = "/data/GameLogs/",
                CertificateFolder = "/data/GameCertificates",
                SharedContentFolder = "/data/GameSharedContent",
                GamePorts = mockPortmapping?.ToDictionary(x => x.GamePort.Name, x => x.GamePort.Number.ToString()),
                PublicIpV4Address = TestPublicIpV4Address,
                GameServerConnectionInfo = new GameServerConnectionInfo
                {
                    PublicIpV4Adress = TestPublicIpV4Address,
                    GamePortsConfiguration = new List<GamePort>()
                    {
                        new GamePort()
                        {
                            Name = mockPortmapping[0].GamePort.Name,
                            ServerListeningPort = mockPortmapping[0].GamePort.Number,
                            ClientConnectionPort = mockPortmapping[0].PublicPort
                        }
                    }
                }
            };

            GsdkConfiguration gsdkConfig = JsonConvert.DeserializeObject<GsdkConfiguration>(File.ReadAllText(gsdkConfigFilePath));

            gsdkConfig.Should().BeEquivalentTo(gsdkConfigExpected);
        }
    }
}