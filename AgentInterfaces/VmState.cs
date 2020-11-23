// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using ProtoBuf;

    [JsonConverter(typeof(StringEnumConverter))]
    [ProtoContract]
    public enum VmState
    {
        [ProtoEnum]
        Unknown,

        /// <summary>
        /// The VM has booted up and is waiting to be assigned to a title.
        /// </summary>
        [ProtoEnum]
        Unassigned,

        /// <summary>
        /// The VM is assigned and ready to download game resources (such as assets and container images).
        /// </summary>
        [ProtoEnum]
        Assigned,

        /// <summary>
        /// Assets and game packages are being downloaded.
        /// </summary>
        [ProtoEnum]
        Propping,

        /// <summary>
        /// Retrieving the assets and/or container image failed.
        /// </summary>
        [ProtoEnum]
        ProppingFailed,

        /// <summary>
        /// Retrieving the assets and containers completed.
        /// </summary>
        [ProtoEnum]
        ProppingCompleted,

        /// <summary>
        /// Starting the game servers failed (all of them) due to platform errors (such as docker issues).
        /// </summary>
        [Obsolete]
        [ProtoEnum]
        ServerStartFailed,

        /// <summary>
        /// Starting one or more game servers failed due to platform errors (such as docker issues).
        /// </summary>
        [ProtoEnum]
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
        [ProtoEnum]
        PartiallyRunning,

        /// <summary>
        /// One or more containers have successfully started.
        /// </summary>
        [ProtoEnum]
        Running,

        /// <summary>
        /// The VM is marked for unassignment and is pending removal of containers, assets and images.
        /// </summary>
        [ProtoEnum]
        PendingResourceCleanup,

        /// <summary>
        /// As part of unassignment, the session hosts (containers) were cleaned up.
        /// At this point, it is safe to remove resources such as asset folders and container images.
        /// </summary>
        [Obsolete]
        [ProtoEnum]
        SessionHostsRemoved,

        /// <summary>
        /// As part of unassignment, the session hosts (containers) were cleaned up.
        /// At this point, it is safe to remove resources such as asset folders and container images.
        /// </summary>
        [ProtoEnum]
        ServersRemoved,

        /// <summary>
        /// The game servers are exiting too quickly (potentially due to crashes).
        /// </summary>
        [ProtoEnum]
        TooManyServerRestarts
    }
}
