using System;
using System.Collections.Generic;
using System.Text;

namespace VmAgent.Core.UnitTests
{
    using System.Globalization;
    using FluentAssertions;
    using Microsoft.Azure.Gaming.AgentInterfaces;
    using Microsoft.Azure.Gaming.VmAgent.Core;
    using Microsoft.Azure.Gaming.VmAgent.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VmConfigurationTests
    {
        private static readonly string PlayFabTitleId = "ABC123";

        private static readonly Guid DeploymentId = Guid.NewGuid();

        private static readonly string Region = "SouthCentralUs";

        private static readonly VmConfiguration VmConfiguration = new VmConfiguration(56001, Guid.NewGuid().ToString(), new VmDirectories("C:\\windows\\temp"));

        [TestMethod]
        [TestCategory("BVT")]
        public void EnvVariablesWithBuildMetadata()
        {
            var metadata = new Dictionary<string, string>() {{"key1", "value1"}, {"key2", "value2"}};
            SessionHostsStartInfo sessionHostsStartInfo = CreateSessionHostStartInfo(metadata);
            IDictionary<string, string> envVariables = VmConfiguration.GetCommonEnvironmentVariables(sessionHostsStartInfo, VmConfiguration);
            ValidateCommonEnvironmentVariables(envVariables, sessionHostsStartInfo);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void EnvVariablesWithoutBuildMetadata()
        {
            SessionHostsStartInfo sessionHostsStartInfo = CreateSessionHostStartInfo();
            IDictionary<string, string> envVariables = VmConfiguration.GetCommonEnvironmentVariables(sessionHostsStartInfo, VmConfiguration);
            ValidateCommonEnvironmentVariables(envVariables, sessionHostsStartInfo);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void VmScriptEnvVariablesWithBuildMetadata()
        {
            var metadata = new Dictionary<string, string>() { { "key1", "value1" }, { "key2", "value2" } };
            SessionHostsStartInfo sessionHostsStartInfo = CreateSessionHostStartInfo(metadata);
            IDictionary<string, string> envVariables = VmConfiguration.GetEnvironmentVariablesForVmScripts(sessionHostsStartInfo, VmConfiguration);
            ValidateVmScriptEnvironmentVariables(envVariables, sessionHostsStartInfo);
        }

        private SessionHostsStartInfo CreateSessionHostStartInfo(IDictionary<string, string> buildMetadata = null)
        {
            return new SessionHostsStartInfo
            {
                AssignmentId = CreateAssignmentId(), DeploymentMetadata = buildMetadata, PublicIpV4Address = "42.42.42.42."
            };
        }

        private void ValidateCommonEnvironmentVariables(IDictionary<string, string> envVariables, SessionHostsStartInfo sessionHostsStartInfo)
        {
            envVariables.Should().Contain(VmConfiguration.PublicIPv4AddressEnvVariable, sessionHostsStartInfo.PublicIpV4Address);
            envVariables.Should().Contain(VmConfiguration.PublicIPv4AddressEnvVariableV2, sessionHostsStartInfo.PublicIpV4Address);
            envVariables.Should().Contain(VmConfiguration.FqdnEnvVariable, sessionHostsStartInfo.FQDN);
            envVariables.Should().Contain(VmConfiguration.TitleIdEnvVariable, PlayFabTitleId);
            envVariables.Should().Contain(VmConfiguration.BuildIdEnvVariable, DeploymentId.ToString());
            envVariables.Should().Contain(VmConfiguration.RegionEnvVariable, Region);
            sessionHostsStartInfo.DeploymentMetadata?.ForEach(x => envVariables.Should().Contain(x.Key, x.Value));
        }

        private void ValidateVmScriptEnvironmentVariables(IDictionary<string, string> envVariables, SessionHostsStartInfo sessionHostsStartInfo)
        {
            ValidateCommonEnvironmentVariables(envVariables, sessionHostsStartInfo);
            envVariables.Should().Contain(VmConfiguration.SharedContentFolderVmVariable, VmConfiguration.VmDirectories.GameSharedContentFolderVm);
        }

        private string CreateAssignmentId()
        {
            var titleIdGuid = VmConfiguration.GetGuidFromTitleId(ulong.Parse(PlayFabTitleId, NumberStyles.HexNumber));
            return $"{titleIdGuid}{VmConfiguration.AssignmentIdSeparator}{DeploymentId}{VmConfiguration.AssignmentIdSeparator}{Region}";
        }
    }
}
