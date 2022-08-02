// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.DeploymentTool
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using PlayFab.MultiplayerModels;


    public class DeploymentSettings
    {
        /// <summary>
        /// The build name.
        /// </summary>
        public string BuildName { get; set; }

        /// <summary>
        /// The VM size to create the build on.
        /// </summary>
        public string VmSize { get; set; }

        /// <summary>
        /// The number of multiplayer servers to host on a single VM.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int MultiplayerServerCountPerVm { get; set; }

        /// <summary>
        /// Gets or sets the Linux instrumentation configuration for this deployment.
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<BuildRegionParams> RegionConfigurations { get; } = new List<BuildRegionParams>();

        /// <summary>
        /// When true, assets will not be copied for each server inside the VM. All servers
        /// will run from the same set of assets, or will have the same assets mounted in the container.
        /// </summary>
        public bool? AreAssetsReadonly { get; set; } = false;

        /// <summary>
        /// When true, assets will be downloaded and uncompressed in memory, without the compressed
        /// version being written first to disc.
        /// </summary>
        public bool? UseStreamingForAssetDownloads { get; set; } = false;

        /// <summary>
        /// Gets or sets the crash dump configuration for this deployment.
        /// This is only required for Windows Containers
        /// </summary>
        public WindowsCrashDumpConfiguration WindowsCrashDumpConfiguration { get; set; } = new WindowsCrashDumpConfiguration();
    }
}
