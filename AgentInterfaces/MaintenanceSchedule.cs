// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    // Data Format: https://docs.microsoft.com/en-us/azure/virtual-machines/virtual-machines-scheduled-events
    public class MaintenanceSchedule
    {
        public string DocumentIncarnation { get; set; }

        [JsonProperty("Events")]
        public IList<MaintenanceEvent> MaintenanceEvents { get; set; }

        public string ReportingVmId { get; set; }
    }

    public class MaintenanceEvent
    {
        public string EventId { get; set; }

        public string EventType { get; set; }

        public string ResourceType { get; set; }

        [JsonProperty("Resources")]
        public IList<string> AffectedResources { get; set; }

        public string EventStatus { get; set; }

        public DateTime? NotBefore { get; set; }
    }
}
