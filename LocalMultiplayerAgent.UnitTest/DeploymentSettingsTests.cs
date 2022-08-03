using FluentAssertions;
using Microsoft.Azure.Gaming.LocalMultiplayerAgent.DeploymentTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.UnitTests
{
    [TestClass]
    public class DeploymentSettingsTests
    {
        private const string DefaultConfig = @"{
            ""BuildName"": ""GameProcess"",
            ""VmSize"": ""Standard_D2_v2"",
            ""MultiplayerServerCountPerVm"": 5,
            ""RegionConfigurations"": [
                {
                    ""Region"": ""EastUs"",
                    ""MaxServers"": 2,
                    ""StandbyServers"": 1
                }
            ]
        }";

        private dynamic GetValidConfig()
        {
            dynamic config = JObject.Parse(DefaultConfig);

            return config;
        }

        private DeploymentSettings GetTestDeploymentSettings()
        {
            dynamic config = GetValidConfig();
            DeploymentSettings settings = JsonConvert.DeserializeObject<DeploymentSettings>(config.ToString());

            return settings;
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void NullConfigReturnsException()
        {
            Action comparison = () => { DeploymentSettingsValidator validator = new DeploymentSettingsValidator(null); };
            comparison.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void ValidConfigReturnsValid()
        {
            DeploymentSettings settings = GetTestDeploymentSettings();
            new DeploymentSettingsValidator(settings).IsValid().Should().BeTrue();
        }
        
        [TestMethod]
        [TestCategory("BVT")]
        public void EmptyRegionConfigIsInValid()
        {
            DeploymentSettings settings = GetTestDeploymentSettings();

            settings.RegionConfigurations.Clear();
            new DeploymentSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InValidVmSizeReturnsInValid()
        {
            DeploymentSettings settings = GetTestDeploymentSettings();

            settings.VmSize = settings.VmSize.ToLower();
            new DeploymentSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InValidRegionConfigIsInValid()
        {
            DeploymentSettings settings = GetTestDeploymentSettings();

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
            DeploymentSettings settings = GetTestDeploymentSettings();

            settings.MultiplayerServerCountPerVm = -1;
            new DeploymentSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InvalidBuildNameIsInvalid()
        {
            DeploymentSettings settings = GetTestDeploymentSettings();

            settings.BuildName = null;
            new DeploymentSettingsValidator(settings).IsValid().Should().BeFalse();
        }
    }
}
