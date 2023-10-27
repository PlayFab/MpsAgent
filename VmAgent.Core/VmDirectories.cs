// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    public class VmDirectories
    {
        // Temp storage on Azure VM: windows: D drive. On linux, /dev/sdb1 is the disk, but the mounted filesystem is /mnt
        public string TempStorageRootVm { get; }

        // Unfortunately can't re-use the same directory path as above, since they don't necessarily exist on the container
        public static readonly string TempStorageRootContainer = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\"
            : "/data/";

        public static readonly string PlayFabFolderOnPrimaryDrive = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\playfab\"
                : "/playfab/";

        public VmDirectories(string rootPath)
        {
            TempStorageRootVm = rootPath;

            GameSharedContentFolderVm = Path.Combine(TempStorageRootVm, "GameSharedContent");
            GameLogsRootFolderVm = Path.Combine(TempStorageRootVm, "GameLogs");
            DumpsRootFolderVm = Path.Combine(TempStorageRootVm, "Dumps");
            AssetExtractionRootFolderVm = Path.Combine(TempStorageRootVm, "ExtAssets");
            AssetDownloadRootFolderVm = Path.Combine(TempStorageRootVm, "DownloadedAssets");
            CertificateRootFolderVm = Path.Combine(TempStorageRootVm, "GameCertificates");
            GsdkConfigRootFolderVm = Path.Combine(TempStorageRootVm, "Config");

            GameSharedContentFolderContainer = Path.Combine(TempStorageRootContainer, "GameSharedContent");
            GameLogsRootFolderContainer = Path.Combine(TempStorageRootContainer, "GameLogs");
            CertificateRootFolderContainer = Path.Combine(TempStorageRootContainer, "GameCertificates");

            GsdkConfigRootFolderContainer = Path.Combine(TempStorageRootContainer, "Config");
            GsdkConfigFilePathContainer = Path.Combine(GsdkConfigRootFolderContainer, GsdkConfigFilename);

            AgentStateFile = Path.Combine(TempStorageRootVm, "PlayFabVmAgentState");
            AgentStateTempFile = Path.Combine(TempStorageRootVm, "PlayFabVmAgentState.tmp");
            MonitoringStateFile = Path.Combine(TempStorageRootVm, "PlayFabMonitoringState");
            MonitoringStateTempFile = Path.Combine(TempStorageRootVm, "PlayFabMonitoringState.tmp");
            HostConfigOverrideFile = Path.Combine(TempStorageRootVm, "HostConfigOverride.json");
            AgentLogsFolder = Path.Combine(TempStorageRootVm, "PlayFabVmAgentLogs");
            MonitoringAssetInstallationFolder = Path.Combine(TempStorageRootVm, "MonitoringApplication");
            MonitoringOutputFolder = Path.Combine(TempStorageRootVm, "MonitoringApplicationOutput");
            MonitoringKillSentinelFolder = Path.Combine(TempStorageRootVm, "MonitoringSentinelFolder");
            VmStartupScriptFolder = Path.Combine(TempStorageRootVm, "VmStartupScript");
        }

        public string AgentStateFile { get; }

        public string AgentStateTempFile { get; }

        public string HostConfigOverrideFile { get; }

        public string AgentLogsFolder { get; }

        // A folder that is accessible by all games on the Vm, potentially for content that's downloaded once but used multiple times.
        public string GameSharedContentFolderVm { get; }

        public string GameSharedContentFolderContainer { get; set; }

        public string GameLogsRootFolderVm { get; }

        // Only used on Process-based servers. All dumps are placed in this folder, and then must be moved into
        // the corresponding server's logs/_dumps folder during log upload.
        public string DumpsRootFolderVm { get; }

        public string GameLogsRootFolderContainer { get; set; }

        public const string GameDumpsFolderName = "_dumps";

        public string AssetExtractionRootFolderVm { get; }

        public string AssetDownloadRootFolderVm { get; }

        public string CertificateRootFolderVm { get; }

        public string CertificateRootFolderContainer { get; set; }

        public const string GsdkConfigFilename = "gsdkConfig.json";

        public string GsdkConfigRootFolderVm { get; }

        public string GsdkConfigRootFolderContainer { get; set; }

        public string GsdkConfigFilePathContainer { get; set; }

        public string MonitoringAssetInstallationFolder { get; set; }

        public string MonitoringOutputFolder { get; set; }

        public string MonitoringStateFile { get; set; }

        public string MonitoringStateTempFile { get; set; }

        public string MonitoringKillSentinelFolder { get; set; }

        public string VmStartupScriptFolder { get; set; }
    }
}
