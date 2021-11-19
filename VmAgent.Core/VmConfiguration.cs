// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AgentInterfaces;
    using VmAgent.Extensions;


    public class VmConfiguration
    {
        // The values prefixed with "PF" can potentially be used by game server.
        // The values that are not prefixed with PF are used by GSDK and container startup script.
        // Used by the GSDK and game start scripts to get the Title Id for this session host
        public const string TitleIdEnvVariable = "PF_TITLE_ID";

        // Used by the GSDK and game start scripts to get the Build Id for this session host
        public const string BuildIdEnvVariable = "PF_BUILD_ID";

        // Used by the GSDK and game start scripts to get the Azure Region for this session host
        public const string RegionEnvVariable = "PF_REGION";

        // The VmId of the VM that the session host is running on.
        public const string VmIdEnvVariable = "PF_VM_ID";

        // Some legacy games ping themselves over the internet for health monitoring.
        // In order to set up etc\hosts file for those games, we provide the public IP via an
        // environment variable.
        public const string PublicIPv4AddressEnvVariable = "PUBLIC_IPV4_ADDRESS";

        // This is same as PublicIPv4AddressEnvVariable but with a prefix to standardize env variables.
        public const string PublicIPv4AddressEnvVariableV2 = "PF_PUBLIC_IPV4_ADDRESS";

        public const string FqdnEnvVariable = "PF_FQDN";

        private static readonly byte[] PlayFabTitleIdPrefix = BitConverter.GetBytes(0xFFFFFFFFFFFFFFFF);

        public int ListeningPort { get; }

        public string VmId { get; }

        public VmDirectories VmDirectories { get; }

        public bool RunContainersInUserMode { get; }

        public VmConfiguration(int listeningPort, string vmId, VmDirectories vmDirectories, bool runContainersInUserMode = false)
        {
            ListeningPort = listeningPort;
            VmId = vmId;
            VmDirectories = vmDirectories;
            RunContainersInUserMode = runContainersInUserMode;
        }

        public const string AssignmentIdSeparator = ":";

        /// <summary>
        /// Gets the set of environment variables that's common to scripts running at the VM level and for game servers,
        /// </summary>
        /// <param name="sessionHostsStartInfo">The details for starting the game servers.</param>
        /// <param name="vmConfiguration">The details for the VM.</param>
        /// <returns>A dictionary of environment variables</returns>
        /// <remarks>This method is expected to be called only after the VM is assigned (i.e sessionHostsStartInfo is not null).</remarks>
        public static IDictionary<string, string> GetCommonEnvironmentVariables(SessionHostsStartInfo sessionHostsStartInfo, VmConfiguration vmConfiguration)
        {
            VmConfiguration.ParseAssignmentId(sessionHostsStartInfo.AssignmentId, out Guid titleId, out Guid deploymentId, out string region);
            var environmentVariables = new Dictionary<string, string>()
            {
                {
                    TitleIdEnvVariable, VmConfiguration.GetPlayFabTitleId(titleId)
                },
                {
                    BuildIdEnvVariable, deploymentId.ToString()
                },
                {
                    RegionEnvVariable, region
                },
                {
                    VmIdEnvVariable, vmConfiguration.VmId
                },
                {
                    PublicIPv4AddressEnvVariable, sessionHostsStartInfo.PublicIpV4Address
                },
                {
                    PublicIPv4AddressEnvVariableV2, sessionHostsStartInfo.PublicIpV4Address
                },
                {
                    FqdnEnvVariable, sessionHostsStartInfo.FQDN
                }
            };

            sessionHostsStartInfo.DeploymentMetadata?.ForEach(x => environmentVariables.Add(x.Key, x.Value));

            return environmentVariables;
        }

        public static void ParseAssignmentId(string assignmentId, out Guid titleId, out Guid deploymentId, out string region)
        {
            region = string.Empty;
            string[] parts = assignmentId.Split(AssignmentIdSeparator);
            titleId = Guid.Parse(parts[0]);
            deploymentId = Guid.Parse(parts[1]);
            region = parts[2];
        }

        // TODO consider moving it to a Core nuget package for control plane and VmAgent (or just put it in AgentInterfaces).

        public static string GetPlayFabTitleId(Guid titleId)
        {
            // The first eight bytes are prefixed with 'ff' for PlayFab titles.
            return BitConverter.ToUInt64(titleId.ToByteArray(), 8).ToString("X");
        }

        public static Guid GetGuidFromTitleId(ulong titleId)
        {
            // The first eight bytes are prefixed with 'ff' for PlayFab titles.
            return new Guid(PlayFabTitleIdPrefix.Concat(BitConverter.GetBytes(titleId)).ToArray());
        }

        public string GetAssetDownloadFileName(string assetBlobName)
        {
            return Path.Combine(VmDirectories.AssetDownloadRootFolderVm, assetBlobName);
        }

        /// <summary>
        ///     Gets the folder where the assets must be extracted to, for the specified session host instance.
        /// </summary>
        /// <param name="sessionHostInstance"></param>
        /// <remarks>
        ///     Each session host potentially has its own copy of assets. In such a case,
        ///     the assets for each session host instance is designated in specific folders calculated in this method.
        /// </remarks>
        private string GetAssetsExtractionRootFolderForSessionHost(int sessionHostInstance)
        {
            return Path.Combine(VmDirectories.AssetExtractionRootFolderVm, $"SH{sessionHostInstance}");
        }

        public string GetConfigRootFolderForSessionHost(int sessionHostInstance)
        {
            return Path.Combine(VmDirectories.GsdkConfigRootFolderVm, $"SH{sessionHostInstance}");
        }

        /// <summary>
        ///     Gets the asset folder path for the specified host number and asset number.
        /// </summary>
        /// <param name="sessionHostInstance">The sessionHost number.</param>
        /// <param name="assetNumber">The asset number.</param>
        /// <remarks>
        ///     Consider a case where the VM has 2 session hosts and 2 assets for each.
        ///     An input of (0, 0), returns the following path: D:\ExtAssets\SH0\A0.
        ///     An input of (0, 1), returns the following path: D:\ExtAssets\SH0\A1.
        ///     An input of (1, 0), returns the following path: D:\ExtAssets\SH1\A0.
        ///     And so on. The paths above are for windows, a similar pattern is followed for Linux.
        /// </remarks>
        public string GetAssetExtractionFolderPathForSessionHost(int sessionHostInstance, int assetNumber)
        {
            return Path.Combine(GetAssetsExtractionRootFolderForSessionHost(sessionHostInstance), $"A{assetNumber}");
        }
    }
}
