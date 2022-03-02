// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SessionHostStatus
    {
        /// <summary>
        /// The server state is unknown/invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// The server has been started by the VmAgent but hasn't sent a heartbeat yet.
        /// </summary>
        PendingHeartbeat,

        /// <summary>
        /// The server is initializing game data.
        /// </summary>
        Initializing,

        /// <summary>
        /// The server is ready for players to connect.
        /// </summary>
        StandingBy,

        /// <summary>
        /// The server has been allocated and is in session.
        /// </summary>
        Active,

        /// <summary>
        /// The server is terminating.
        /// </summary>
        Terminating,

        /// <summary>
        /// The server has terminated.
        /// </summary>
        Terminated,

        /// <summary>
        /// Legacy value, unclear if useful in PlayFab (could be useful if server is failing health checks once that's implemented).
        /// </summary>
        Quarantined
    }
}
