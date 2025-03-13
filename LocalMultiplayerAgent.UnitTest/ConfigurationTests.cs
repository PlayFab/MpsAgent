// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.UnitTests
{
    using System;
    using System.Reflection;
    using AgentInterfaces;
    using Core.Interfaces;
    using FluentAssertions;
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
    }
}
