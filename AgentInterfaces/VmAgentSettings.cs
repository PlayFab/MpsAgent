// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.Collections.Generic;

    public class VmAgentSettings
    {
        public int MaxLogUploadTimeInSeconds { get; set; }

        /// <summary>
        /// Titles that need to have the port that clients connects to on the Load Balancer be the same as the one that the server listens
        /// to, on the VM. This is because the game server has been written to handle only one port, to listen to and report the same one to a different
        /// matchmaking/allocation service (which would would then talk to the game server over the public port).
        /// </summary>
        public HashSet<string> TitlesNeedingPublicPortToMatchGamePort { get; set; }

        /// <summary>
        /// Titles that do not use PlayFab MultiPlayer Servers for allocating the servers, and only use it for scaling.
        /// </summary>
        public HashSet<string> TitlesUsingExternalAllocations { get; set; }

        /// <summary>
        ///  Whether the maintenance schedule time should be passed to GSDK.
        /// </summary>
        public bool ShouldPassMaintenanceInfoToGsdk { get; set; }

        /// <summary>
        /// Whether to retain the assets (to prevent a re-download) when a VM gets reassigned after being unassigned.
        /// </summary>
        public bool ShouldRetainAssetsOnReassignment { get; set; }

        /// <summary>
        /// Whether we should stop bringing up new session hosts (when old ones terminate), if the VM has been scheduled for maintenance.
        /// </summary>
        public bool DisableSessionHostCreationOnMaintenanceEvent { get; set; }

        /// <summary>
        /// ControlPlane can use this field to notify VmAgent that it will shut down soon.
        /// This isn't really a "setting", and isn't stored in Thunderfig. VmAgentSettings is just a convenient way for 
        /// ControlPlane to communicate with VmAgent.
        /// </summary>
        public DateTimeOffset? ShutdownRequestedNotBefore { get; set; }
    }
}
