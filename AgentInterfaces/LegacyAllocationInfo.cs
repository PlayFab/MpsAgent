// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System.Collections.Generic;
    using ProtoBuf;

    [ProtoContract]
    public class LegacyAllocationInfo
    {
        [ProtoMember(1)]
        public IDictionary<string, string> ClusterManifest { get; set; }
    }
}
