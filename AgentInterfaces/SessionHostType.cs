// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Captures the various mechanisms in which a game server is hosted within Thunderhead.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SessionHostType
    {
        /// <summary>
        ///     Each session host is a (docker) container within the VM.
        /// </summary>
        Container,

        /// <summary>
        ///     Each session host is a process.
        /// </summary>
        Process
    }
}
