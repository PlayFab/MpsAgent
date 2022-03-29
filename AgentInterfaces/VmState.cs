// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum VmState
    {
        Unknown,

        /// <summary>
        /// The VM has booted up and is waiting to be assigned to a title.
        /// </summary>
        Unassigned,

        /// <summary>
        /// The VM is assigned and ready to download game resources (such as assets and container images).
        /// </summary>
        Assigned,

        /// <summary>
        /// Assets and game packages are being downloaded.
        /// </summary>
        Propping,

        /// <summary>
        /// Retrieving the assets and/or container image failed.
        /// </summary>
        ProppingFailed,

        /// <summary>
        /// Retrieving the assets and containers completed.
        /// </summary>
        ProppingCompleted,

        /// <summary>
        /// Starting the game servers failed (all of them) due to platform errors (such as docker issues).
        /// </summary>
        [Obsolete]
        ServerStartFailed,

        /// <summary>
        /// Starting one or more game servers failed due to platform errors (such as docker issues).
        /// </summary>
        StartServersFailed,

        /// <summary>
        /// Some of the containers were started, others are pending start or have failed to start.
        /// </summary>
        /// <remarks>
        /// This is not necessarily an error state. It is possible that some containers
        /// start reporting heartbeat while others are still being created/started. In that case,
        /// the VM could be still marked as PartiallyRunning.
        /// </remarks>
        [Obsolete]
        PartiallyRunning,

        /// <summary>
        /// One or more containers have successfully started.
        /// </summary>
        Running,

        /// <summary>
        /// The VM is marked for unassignment and is pending removal of containers, assets and images.
        /// </summary>
        PendingResourceCleanup,

        /// <summary>
        /// As part of unassignment, the session hosts (containers) were cleaned up.
        /// At this point, it is safe to remove resources such as asset folders and container images.
        /// </summary>
        [Obsolete]
        SessionHostsRemoved,

        /// <summary>
        /// As part of unassignment, the session hosts (containers) were cleaned up.
        /// At this point, it is safe to remove resources such as asset folders and container images.
        /// </summary>
        ServersRemoved,

        /// <summary>
        /// The game servers are exiting too quickly (potentially due to crashes).
        /// </summary>
        TooManyServerRestarts
    }
}
