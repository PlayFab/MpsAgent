// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.DeploymentTool
{
    //using System;
    using System.Collections.Generic;
    //using AgentInterfaces;
    using PlayFab.MultiplayerModels;

    public class DeploymentSettings
    {
        public string BuildName { get; set; }

        public string VmSize { get; set; }

        public int MultiplayerServerCountPerVm { get; set; }

        public string OSPlatform { get; set; }

        public List<string> AssetFileNames { get; set; }

        public List<BuildRegionParams> RegionConfigurations { get; set; }
    }
}
