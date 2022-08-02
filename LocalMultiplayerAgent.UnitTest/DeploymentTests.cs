using FluentAssertions;
using Microsoft.Azure.Gaming.LocalMultiplayerAgent.Config;
using Microsoft.Azure.Gaming.LocalMultiplayerAgent.DeploymentTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;*/

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.UnitTests
{
    [TestClass]
    internal class DeploymentTests
    {
        private const string DefaultConfig = @"{
            ""BuildName"": ""GameProcess"",
            ""VmSize"": ""Standard_D2_v2"",
            ""MultiplayerServerCountPerVm"": 10,
            ""RegionConfigurations"": [
                {
                    ""Region"": ""EastUs"",
                    ""MaxServers"": 2,
                    ""StandbyServers"": 1
                }
            ]
        }";

        //private readonly Mock<MultiplayerSettings> mockMultiplayerSettings = new Mock<MultiplayerSettings>();
        //private Mock<ISystemOperations> _mockSystemOperations = new Mock<ISystemOperations>();

        private dynamic GetValidConfig()
        {
            dynamic config = JObject.Parse(DefaultConfig);

            return config;
        }

        [TestInitialize]
        public void BeforeEachTest()
        {
            //mockMultiplayerSettings.Setup(x => x.).Returns(true);
            /*_mockSystemOperations.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            _mockSystemOperations.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);*/
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void ValidConfigReturnsValid()
        {
            dynamic config = GetValidConfig();
            DeploymentSettings settings = JsonConvert.DeserializeObject<DeploymentSettings>(config.ToString());
            new DeploymentSettingsValidator(settings).IsValid().Should().BeTrue();
        }
        
        [TestMethod]
        [TestCategory("BVT")]
        public void EmptyRegionConfigIsInValid()
        {
            dynamic config = GetValidConfig();
            DeploymentSettings settings = JsonConvert.DeserializeObject<DeploymentSettings>(config.ToString());

            settings.RegionConfigurations.Clear();
            new DeploymentSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InValidVmSizeReturnsInValid()
        {
            dynamic config = GetValidConfig();
            DeploymentSettings settings = JsonConvert.DeserializeObject<DeploymentSettings>(config.ToString());

            settings.VmSize = settings.VmSize.ToLower();
            
            new DeploymentSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InValidRegionConfigIsInValid()
        {
            dynamic config = GetValidConfig();
            DeploymentSettings settings = JsonConvert.DeserializeObject<DeploymentSettings>(config.ToString());

            foreach (var region in settings.RegionConfigurations)
            {
                region.Region = region.Region.ToLower();
            }
            
            new DeploymentSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InvalidServerCountPerVmIsInvalid()
        {
            dynamic config = GetValidConfig();

            DeploymentSettings settings = JsonConvert.DeserializeObject<DeploymentSettings>(config.ToString());
            settings.MultiplayerServerCountPerVm = -1;
            new DeploymentSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InvalidBuildNameIsInvalid()
        {
            dynamic config = GetValidConfig();

            DeploymentSettings settings = JsonConvert.DeserializeObject<DeploymentSettings>(config.ToString());
            settings.BuildName = null;
            
            new DeploymentSettingsValidator(settings).IsValid().Should().BeFalse();
        }
    }
}
