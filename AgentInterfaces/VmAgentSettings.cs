// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System.Collections.Generic;

    public class VmAgentSettings
    {
        public bool EnableCrashDumpProcessing { get; set; }

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
    }
}
