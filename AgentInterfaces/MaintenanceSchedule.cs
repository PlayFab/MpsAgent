// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    // Data Format: https://docs.microsoft.com/en-us/azure/virtual-machines/windows/scheduled-events
    [Serializable]
    public class MaintenanceSchedule
    {
        public string DocumentIncarnation { get; set; }

        [JsonProperty("events")]
        public IList<MaintenanceEvent> MaintenanceEvents { get; set; }

        public MaintenanceSchedule() { }

        /// <summary>
        /// Deep copy used for testing
        /// </summary>
        public MaintenanceSchedule(MaintenanceSchedule other)
        {
            DocumentIncarnation = other.DocumentIncarnation;
            MaintenanceEvents = other.MaintenanceEvents.Select((e) => new MaintenanceEvent(e)).ToList();
        }
    }

    // https://docs.microsoft.com/en-us/azure/virtual-machines/windows/scheduled-events#query-for-events
    [Serializable]
    public class MaintenanceEvent
    {
        public string EventId { get; set; }

        public string EventType { get; set; }

        public string ResourceType { get; set; }

        [JsonProperty("resources")]
        public IList<string> AffectedResources { get; set; }

        public string EventStatus { get; set; }

        public DateTime? NotBefore { get; set; }

        public string Description { get; set; }

        public string EventSource { get; set; }

        public int DurationInSeconds { get; set; }

        public MaintenanceEvent() { }

        /// <summary>
        /// Deep copy used for testing
        /// </summary>
        public MaintenanceEvent(MaintenanceEvent other)
        {
            EventId = other.EventId;
            EventType = other.EventType;
            ResourceType = other.ResourceType;
            AffectedResources = other.AffectedResources.ToList();
            EventStatus = other.EventStatus;
            NotBefore = other.NotBefore;
            EventSource = other.EventSource;
            DurationInSeconds = other.DurationInSeconds;
        }
    }
}
