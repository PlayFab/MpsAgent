﻿using FluentAssertions;
using Microsoft.Azure.Gaming.LocalMultiplayerAgent.BuildTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.UnitTests
{
    [TestClass]
    public class BuildSettingsTests
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

        private BuildSettings GetTestDeploymentSettings()
        {
            dynamic config = GetValidConfig();
            BuildSettings settings = JsonConvert.DeserializeObject<BuildSettings>(config.ToString());

            return settings;
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void NullConfigReturnsException()
        {
            Action comparison = () => { BuildSettingsValidator validator = new BuildSettingsValidator(null); };
            comparison.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void ValidConfigReturnsTrue()
        {
            BuildSettings settings = GetTestDeploymentSettings();
            new BuildSettingsValidator(settings).IsValid().Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void EmptyRegionConfigReturnsFalse()
        {
            BuildSettings settings = GetTestDeploymentSettings();

            settings.RegionConfigurations.Clear();
            new BuildSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InvalidVmSizeReturnsFalse()
        {
            BuildSettings settings = GetTestDeploymentSettings();

            settings.VmSize = settings.VmSize.ToLower();
            new BuildSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InvalidRegionConfigReturnsFalse()
        {
            BuildSettings settings = GetTestDeploymentSettings();

            foreach (var region in settings.RegionConfigurations)
            {
                region.Region = region.Region.ToLower();
            }

            new BuildSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        [DataRow("BrazilSouth")]
        [DataRow("WestCentralUS")]
        [DataRow("USDoDCentral")]
        [DataRow("EastUS2EUAP")]
        public void InvalidRegionConfigReturnsFalse(string reg)
        {
            BuildSettings settings = GetTestDeploymentSettings();

            foreach (var region in settings.RegionConfigurations)
            {
                region.Region = reg;
            }

            new BuildSettingsValidator(settings).IsValid().Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InvalidServerCountPerVmReturnsFalse()
        {
            BuildSettings settings = GetTestDeploymentSettings();

            settings.MultiplayerServerCountPerVm = -1;
            new BuildSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void InvalidBuildNameReturnsFalse()
        {
            BuildSettings settings = GetTestDeploymentSettings();

            settings.BuildName = null;
            new BuildSettingsValidator(settings).IsValid().Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("BVT")]
        [DataRow("Standard_D2_v2")]
        [DataRow("Standard_F4s_v2")]
        [DataRow("Standard_A1")]
        [DataRow("Standard_HB120_32rs_v3")]
        public void ValidVmSizeInputShouldSucceed(string vmSize)
        {
            dynamic config = GetValidConfig();
            config.vmSize = vmSize;
            BuildSettings settings = JsonConvert.DeserializeObject<BuildSettings>(config.ToString());

            new BuildSettingsValidator(settings).IsValid().Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("BVT")]
        [DataRow("D2_v2")]
        [DataRow(" ")]
        [DataRow("Standard")]
        [DataRow("HB120_32rs_v3")]
        public void InvalidVmSizeInputShouldFail(string vmSize)
        {
            dynamic config = GetValidConfig();
            config.vmSize = vmSize;
            BuildSettings settings = JsonConvert.DeserializeObject<BuildSettings>(config.ToString());

            new BuildSettingsValidator(settings).IsValid().Should().BeFalse();
        }
    }
}