// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.DeploymentTool
{
    using System.Collections.Generic;
    using PlayFab.MultiplayerModels;

    public class DeploymentSettings
    {
        public string BuildName { get; set; }

        public string VmSize { get; set; }

        public int MultiplayerServerCountPerVm { get; set; }

        public List<BuildRegionParams> RegionConfigurations { get; } = new List<BuildRegionParams>();
    }
}
