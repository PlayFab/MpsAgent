// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;

    [ProtoContract]
    public class SessionConfig
    {
        [ProtoMember(1)]
        public Guid SessionId { get; set; }

        [ProtoMember(2)]
        public string SessionCookie { get; set; }

        [ProtoMember(3)]
        public List<string> InitialPlayers { get; set; }

        /// <summary>
        /// Session metadata
        /// </summary>
        [ProtoMember(4)]
        public Dictionary<string, string> Metadata { get; set; }

        [ProtoMember(5)]
        public LegacyAllocationInfo LegacyAllocationInfo { get; set; }

        /// <summary>
        /// Overriden to avoid logging any sensitive information (or large ones session config can be big).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                $"SessionId: {SessionId}; SessionCookieLength: {SessionCookie?.Length.ToString() ?? "NULL"}; InitialPlayersCount: {InitialPlayers?.Count.ToString() ?? "NULL"}; Metadata: {Metadata?.Count.ToString() ?? "NULL"}";
        }
    }
}
