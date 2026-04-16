// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.UnitTests
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using AgentInterfaces;
    using Core.Interfaces;
    using FluentAssertions;
    using LocalMultiplayerAgent;
    using LocalMultiplayerAgent.Config;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigurationTests
    {
        private const string DefaultConfig = @"{
            ""RunContainer"": false,
            ""OutputFolder"": """",
            ""NumHeartBeatsForActivateResponse"": 10,
            ""NumHeartBeatsForTerminateResponse"": 60,
            ""AgentListeningPort"": 56001,
            ""GameCertificateFiles"": [],
            ""AssetDetails"": [
            {
                ""MountPath"": ""C:\\Assets"",
                ""LocalFilePath"": ""<path_to_game_server_package>""
            }
            ],
            ""PortMappingsList"": [
            [
            {
                ""NodePort"": 56100,
                ""GamePort"": {
                    ""Name"": ""gameport"",
                    ""Number"": 3600,
                    ""Protocol"": ""TCP""
                }
            }
            ]
            ],
            ""ProcessStartParameters"": {
                ""StartGameCommand"": ""<your_game_server_exe>""
            },
            ""ContainerStartParameters"": {
                ""StartGameCommand"": ""C:\\Assets\\<your_game_server_exe>"",
                ""ResourceLimits"": {
                    ""Cpus"": 2,
                    ""MemoryGib"": 2
                },
                ""ImageDetails"": {
                    ""Registry"": ""mcr.microsoft.com"",
                    ""ImageName"": ""playfab/multiplayer"",
                    ""ImageTag"": ""wsc-10.0.17134.950"",
                    ""Username"": """",
                    ""Password"": """"
                }
            },
            ""SessionConfig"": {
                ""SessionId"": ""ba67d671-512a-4e7d-a38c-2329ce181946"",
                ""SessionCookie"": null,
                ""InitialPlayers"": [ ""Player1"", ""Player2"" ]
            },
            ""TitleId"": """",
            ""BuildId"": ""00000000-0000-0000-0000-000000000000"",
            ""Region"": ""WestUs""
        }
    ";
        private Mock<ISystemOperations> _mockSystemOperations = new Mock<ISystemOperations>();

        private dynamic GetValidConfig()
        {
            dynamic config = JObject.Parse(DefaultConfig);
            config.AssetDetails[0].LocalFilePath = Assembly.GetExecutingAssembly().Location;
            config.ProcessStartParameters.StartGameCommand = "Game.exe";
            config.ContainerStartParameters.StartGameCommand = @"C:\Assets\Game.exe";

            return config;
        }

        [TestInitialize]
        public void BeforeEachTest()
        {
            _mockSystemOperations.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            _mockSystemOperations.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void DefaultConfigIsInvalid()
        {
            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(DefaultConfig);
            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void ValidConfigReturnsValid()
        {
            // Process mode (RunContainer=false) is not supported on MacOS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Assert.Inconclusive("Process mode validation is not supported on MacOS");
            }

            dynamic config = GetValidConfig();
            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InvalidProcessStartCommandIsInvalid()
        {
            dynamic config = GetValidConfig();
            config.RunContainer = false;
            config.ProcessStartParameters.StartGameCommand = "<your_game_server_exe>";

            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InvalidContainerStartCommandIsInvalid()
        {
            dynamic config = GetValidConfig();
            config.RunContainer = true;
            config.ContainerStartParameters.StartGameCommand = @"C:\Assets\<your_game_server_exe>";

            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void SessionHostStartWithProcess()
        {
            // Process mode (RunContainer=false) is not supported on MacOS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Assert.Inconclusive("Process mode validation is not supported on MacOS");
            }

            dynamic config = GetValidConfig();
            config.RunContainer = false;

            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();

            SessionHostsStartInfo startInfo = settings.ToSessionHostsStartInfo();
            startInfo.HostConfigOverrides.Should().BeNull();
            startInfo.SessionHostType.Should().Be(SessionHostType.Process);
            foreach (AssetDetail assetDetail in startInfo.AssetDetails)
            {
                assetDetail.MountPath.Should().BeNull();
            }
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void SessionHostStartWithContainer()
        {
            dynamic config = GetValidConfig();
            config.RunContainer = true;

            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();

            SessionHostsStartInfo startInfo = settings.ToSessionHostsStartInfo();
            long expectedNanoCpus = (long)((double)config.ContainerStartParameters.ResourceLimits.Cpus * 1_000_000_000);
            long expectedMemory = config.ContainerStartParameters.ResourceLimits.MemoryGib * Math.Pow(1024, 3);
            startInfo.SessionHostType.Should().Be(SessionHostType.Container);
            startInfo.HostConfigOverrides.NanoCPUs.Should().Be(expectedNanoCpus);
            startInfo.HostConfigOverrides.Memory.Should().Be(expectedMemory);
            foreach (AssetDetail assetDetail in startInfo.AssetDetails)
            {
                assetDetail.MountPath.Should().NotBeNull();
            }
        }
        
        [TestMethod]
        [TestCategory("BVT")]
        public void CertsWithSameNameFails()
        {
            dynamic config = GetValidConfig();
            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            settings.GameCertificateDetails = new[]
            {
                new GameCertificateDetails() {Name = "test", Path="C:\\mycert.pfx"}, 
                new GameCertificateDetails() {Name = "test", Path="C:\\mycert2.pfx"}
            };
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
        }
        
        [TestMethod]
        [TestCategory("BVT")]
        public void CertsWithSamePathFails()
        {
            dynamic config = GetValidConfig();
            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            settings.GameCertificateDetails = new[]
            {
                new GameCertificateDetails() {Name = "test", Path="C:\\mycert.pfx"}, 
                new GameCertificateDetails() {Name = "test2", Path="C:\\mycert.pfx"}
            };
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
        }
        
        [TestMethod]
        [TestCategory("BVT")]
        public void CertsWithDifferentPathAndNameSucceeds()
        {
            // Process mode (RunContainer=false) is not supported on MacOS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Assert.Inconclusive("Process mode validation is not supported on MacOS");
            }

            dynamic config = GetValidConfig();
            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            settings.GameCertificateDetails = new[]
            {
                new GameCertificateDetails() {Name = "test", Path="C:\\mycert.pfx"}, 
                new GameCertificateDetails() {Name = "test2", Path="C:\\mycert2.pfx"}
            };
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();
        }
        
        [TestMethod]
        [TestCategory("BVT")]
        public void CertsNoExtensionPfxFails()
        {
            dynamic config = GetValidConfig();
            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            settings.GameCertificateDetails = new[]
            {
                new GameCertificateDetails() {Name="cert1", Path = "C:\\test.pfxlala"}, 
                new GameCertificateDetails() {Name="cert2", Path = "C:\\test2.123"}
            };
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void EmptyNodePortInContainerModeShouldFail()
        {
            dynamic config = GetValidConfig();            
            config.RunContainer = true;
            config.PortMappingsList[0][0].NodePort = 0;
            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());

            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void ValidateNumHeartbeats()
        {
            // test for NumHeartBeatsForActivateResponse
            dynamic config = GetValidConfig();            
            config.NumHeartBeatsForActivateResponse = 0;
            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();

            // test for NumHeartBeatsForTerminateResponse
            config = GetValidConfig();            
            config.NumHeartBeatsForTerminateResponse = 0;
            settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();

            // test for NumHeartBeatsForTerminateResponse and NumHeartBeatsForActivateResponse
            config = GetValidConfig();            
            config.NumHeartBeatsForTerminateResponse = 10;
            config.NumHeartBeatsForActivateResponse = 10;
            settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        [DataRow("powershell.exe D:\\Assets\\GameServer.ps1")]
        [DataRow("powershell.exe E:\\Assets\\GameServer.exe")]
        [DataRow("E:\\MyGameRocks\\GameServer.exe")]
        [DataRow("C:\\Asset\\GameServer.bat")]
        public void StartGameCommandThatDoesNotContainMountPathShouldFail(string startGameCommand)
        {
            dynamic config = GetValidConfig();            
            config.RunContainer = true;
            config.AssetDetails[0].MountPath = "C:\\Assets";
            config.ContainerStartParameters.StartGameCommand = startGameCommand;
            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());

            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();

        }

        [TestMethod]
        [TestCategory("BVT")]
        [DataRow("powershell.exe C:\\Assets\\GameServer.ps1")]
        [DataRow("powershell.exe C:\\Assets\\GameServer.exe")]
        [DataRow("C:\\Assets\\MyGameRocks\\GameServer.exe")]
        [DataRow("C:\\Assets\\GameServer.bat")]
        public void StartGameCommandThatContainsMountPathShouldSucceed(string startGameCommand)
        {
            dynamic config = GetValidConfig();
            config.RunContainer = true;
            config.AssetDetails[0].MountPath = "C:\\Assets";
            config.ContainerStartParameters.StartGameCommand = startGameCommand;
            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());

            settings.SetDefaultsIfNotSpecified();
            new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();

        }

        /// <summary>
        /// When Globals.GameServerEnvironment is Linux and RunContainer is false,
        /// validation should fail on non-Linux OS because Linux game servers require container mode there.
        /// On native Linux, process mode is allowed.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LinuxGameServerEnvironmentWithoutContainerFails()
        {
            // On native Linux OS, process mode is allowed for Linux game servers
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Assert.Inconclusive("On native Linux OS, Linux process mode is valid");
            }

            // On MacOS, process mode is rejected by the macOS-specific check above (line 118-124), which fires first
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Assert.Inconclusive("Process mode validation is not supported on MacOS");
            }

            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Linux;

                dynamic config = GetValidConfig();
                config.RunContainer = false;
                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();
                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// When Globals.GameServerEnvironment is Windows and RunContainer is false (process mode),
        /// validation should succeed — this is the standard Windows process scenario.
        /// This test is skipped on MacOS because the validator rejects RunContainer=false on MacOS.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void WindowsProcessModeIsValid()
        {
            // Process mode (RunContainer=false) is not supported on MacOS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Assert.Inconclusive("Process mode validation is not supported on MacOS");
            }

            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Windows;

                dynamic config = GetValidConfig();
                config.RunContainer = false;
                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();
                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// When Globals.GameServerEnvironment is Windows and RunContainer is true (container mode),
        /// validation should succeed — this is the standard Windows container scenario.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void WindowsContainerModeIsValid()
        {
            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Windows;

                dynamic config = GetValidConfig();
                config.RunContainer = true;
                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();
                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// Validates that StartGameCommand is not required when using Linux game server environment
        /// in container mode (since it can be baked into the container image via CMD/ENTRYPOINT).
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LinuxGameServerEnvironmentAllowsEmptyStartGameCommand()
        {
            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Linux;

                dynamic config = GetValidConfig();
                config.RunContainer = true;
                config.ContainerStartParameters.StartGameCommand = "";
                // Remove asset details (assets are optional for Linux containers)
                config.AssetDetails = new JArray();

                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();

                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// Validates that StartGameCommand IS required when using Windows game server environment.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void WindowsGameServerEnvironmentRequiresStartGameCommand()
        {
            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Windows;

                dynamic config = GetValidConfig();
                config.RunContainer = false;
                config.ProcessStartParameters.StartGameCommand = "";

                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();
                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// Validates that assets are optional for Linux game servers in container mode.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LinuxGameServerAssetsAreOptional()
        {
            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Linux;

                dynamic config = GetValidConfig();
                config.RunContainer = true;
                config.AssetDetails = new JArray();
                config.ContainerStartParameters.StartGameCommand = "/game/server";

                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();

                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// Validates that assets are required for Windows game servers.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void WindowsGameServerAssetsAreRequired()
        {
            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Windows;

                dynamic config = GetValidConfig();
                config.RunContainer = false;
                config.AssetDetails = new JArray();

                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();
                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// On Linux OS, both RunContainer=true (container mode) and RunContainer=false (process mode)
        /// should be accepted.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LinuxOsAcceptsContainerMode()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                // This test only applies when running on Linux OS
                return;
            }

            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Linux;

                dynamic config = GetValidConfig();
                config.RunContainer = true;
                config.AssetDetails = new JArray();
                config.ContainerStartParameters.StartGameCommand = "/game/server";

                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();
                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// On Linux OS, process mode (RunContainer=false) should also be accepted.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LinuxOsAcceptsProcessMode()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                // This test only applies when running on Linux OS
                return;
            }

            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Linux;

                dynamic config = GetValidConfig();
                config.RunContainer = false;

                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();
                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// Linux process mode (RunContainer=false) requires StartGameCommand.
        /// ProcessRunner crashes on null/empty StartGameCommand, so the validator must reject it.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LinuxProcessModeRequiresStartGameCommand()
        {
            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Linux;

                dynamic config = GetValidConfig();
                config.RunContainer = false;
                config.ProcessStartParameters.StartGameCommand = "";

                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();
                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// Linux process mode (RunContainer=false) requires AssetDetails.
        /// ProcessRunner crashes on empty AssetDetails, so the validator must reject it.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LinuxProcessModeRequiresAssets()
        {
            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Linux;

                dynamic config = GetValidConfig();
                config.RunContainer = false;
                config.AssetDetails = new JArray();

                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();
                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeFalse();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// Linux container mode (RunContainer=true) still allows empty StartGameCommand and empty assets.
        /// The Dockerfile provides CMD/ENTRYPOINT and packages all assets into the image.
        /// This test ensures the process-mode fix does not regress container mode.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LinuxContainerModeAllowsEmptyStartGameCommandAndAssets()
        {
            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Linux;

                dynamic config = GetValidConfig();
                config.RunContainer = true;
                config.ContainerStartParameters.StartGameCommand = "";
                config.AssetDetails = new JArray();

                MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(config.ToString());
                settings.SetDefaultsIfNotSpecified();
                new MultiplayerSettingsValidator(settings, _mockSystemOperations.Object).IsValid().Should().BeTrue();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }
    }

    /// <summary>
    /// Tests for NoOpSessionHostManager to verify the LinuxContainersOnWindows property
    /// behaves correctly under different Globals.GameServerEnvironment settings.
    /// </summary>
    [TestClass]
    public class NoOpSessionHostManagerTests
    {
        /// <summary>
        /// When GameServerEnvironment is Windows, LinuxContainersOnWindows should always be false
        /// regardless of the OS platform.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LinuxContainersOnWindows_WindowsEnvironment_ReturnsFalse()
        {
            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Windows;
                var manager = new NoOpSessionHostManager();
                manager.LinuxContainersOnWindows.Should().BeFalse();
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// When GameServerEnvironment is Linux and running on Windows or MacOS,
        /// LinuxContainersOnWindows should return true.
        /// When running on Linux OS, it should return false (native Linux, not "containers on Windows").
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LinuxContainersOnWindows_LinuxEnvironment_ReturnsCorrectValuePerPlatform()
        {
            var previousEnv = Globals.GameServerEnvironment;
            try
            {
                Globals.GameServerEnvironment = GameServerEnvironment.Linux;
                var manager = new NoOpSessionHostManager();

                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    // On native Linux OS, LinuxContainersOnWindows should be false
                    manager.LinuxContainersOnWindows.Should().BeFalse();
                }
                else
                {
                    // On Windows or MacOS, LinuxContainersOnWindows should be true
                    manager.LinuxContainersOnWindows.Should().BeTrue();
                }
            }
            finally
            {
                Globals.GameServerEnvironment = previousEnv;
            }
        }

        /// <summary>
        /// Verifies that a newly created NoOpSessionHostManager can add and track session hosts.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void AddNewSessionHost_CreatesSessionHostWithCorrectState()
        {
            var manager = new NoOpSessionHostManager();
            var sessionHostInfo = manager.AddNewSessionHost("testId", "assignmentId", 0, "logFolderId");

            sessionHostInfo.Should().NotBeNull();
            sessionHostInfo.SessionHostHeartbeatRequest.CurrentGameState.Should().Be(SessionHostStatus.PendingHeartbeat);
        }

        /// <summary>
        /// Verifies that VmState is always Assigned for NoOpSessionHostManager (local testing behavior).
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void GetVmState_ReturnsAssigned()
        {
            var manager = new NoOpSessionHostManager();
            manager.GetVmState().Should().Be(VmState.Assigned);
        }
    }
}
