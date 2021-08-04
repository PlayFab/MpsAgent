// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    using System;
    using System.IO;
    using System.Linq;


    public class VmConfiguration
    {

        private static readonly byte[] PlayFabTitleIdPrefix = BitConverter.GetBytes(0xFFFFFFFFFFFFFFFF);

        public int ListeningPort { get; }

        public string VmId { get; }

        public VmDirectories VmDirectories { get; }

        public bool RunContainersInUsermode { get; }



        public VmConfiguration(int listeningPort, string vmId, VmDirectories vmDirectories, bool runContainersInUserMode)
        {
            ListeningPort = listeningPort;
            VmId = vmId;
            VmDirectories = vmDirectories;
            RunContainersInUsermode = runContainersInUserMode;
        }

        public const string AssignmentIdSeparator = ":";

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
