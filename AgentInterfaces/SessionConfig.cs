// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SessionConfig
    {
        public Guid SessionId { get; set; }

        public string SessionCookie { get; set; }

        public List<string> InitialPlayers { get; set; }

        /// <summary>
        /// Session metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }

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
