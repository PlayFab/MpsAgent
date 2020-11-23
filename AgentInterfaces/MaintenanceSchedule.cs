// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using ProtoBuf;

    // Data Format: https://docs.microsoft.com/en-us/azure/virtual-machines/virtual-machines-scheduled-events
    [ProtoContract]
    public class MaintenanceSchedule
    {
        [ProtoMember(1)]
        public string DocumentIncarnation { get; set; }

        [JsonProperty("Events")]
        [ProtoMember(2)]
        public IList<MaintenanceEvent> MaintenanceEvents { get; set; }

        [ProtoMember(3)]
        public string ReportingVmId { get; set; }
    }

    [ProtoContract]
    public class MaintenanceEvent
    {
        [ProtoMember(1)]
        public string EventId { get; set; }

        [ProtoMember(2)]
        public string EventType { get; set; }

        [ProtoMember(3)]
        public string ResourceType { get; set; }

        [JsonProperty("Resources")]
        [ProtoMember(4)]
        public IList<string> AffectedResources { get; set; }

        [ProtoMember(5)]
        public string EventStatus { get; set; }

        [ProtoMember(6)]
        [CompatibilityLevel(CompatibilityLevel.Level240)]
        public DateTime? NotBefore { get; set; }
    }
}
