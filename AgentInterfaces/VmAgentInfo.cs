// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class VmAgentInfo
    {
        public VmState VmState { get; set; }

        public string AssignmentId { get; set; }

        public IDictionary<string, SessionHostHeartbeatInfo> SessionHostHeartbeatMap { get; set; }

        public MaintenanceSchedule MaintenanceSchedule { get; set; }

        /// <summary>
        /// Sequence number of heartbeat. Newer updates from the same AgentSessionId should have a greater number.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Session id of Agent.  This is reset when the vm agent process restarts.
        /// </summary>
        public Guid AgentProcessGuid { get; set; }

        /// <summary>
        /// Nullable boolean that represents if VM is unassignable or not.
        /// </summary>
        public bool? IsUnassignable { get; set; }

        /// <summary>
        /// Port mappings at the vm level for VmStartupScript
        /// </summary>
        public List<PortMapping> PortMappings { get; set; }

        /// <summary>
        /// The network configuration of the agent, describing the endpoints available on the VM.
        /// </summary>
        public VmNetworkConfiguration NetworkConfiguration { get; set; }

        /// <summary>
        /// Monitoring work product for this VM
        /// </summary>
        public string VmMonitoringOutputId { get; set; }

        public ToSViolationRating ToSViolationRating { get; set; }

        public string ToLogString()
        {
            if (SessionHostHeartbeatMap?.Count > 10)
            {
                string maintenanceSchedule = MaintenanceSchedule?.ToJsonString() ?? string.Empty;
                string networkConfiguration = NetworkConfiguration?.ToJsonString() ?? string.Empty;
                string sessionHostSummary = SessionHostHeartbeatMap.Values.GroupBy(x => x.CurrentGameState).ToDictionary(y => y.Key, y => y.Count()).ToJsonString();
                string portMappings = PortMappings?.ToJsonString() ?? string.Empty;
                return
                    $"VmState: {VmState}, AssignmentId: {AssignmentId ?? string.Empty}, AgentProcessGuid : {AgentProcessGuid}, SequenceNumber {SequenceNumber}, MaintenanceSchedule : {maintenanceSchedule}, IsUnassignable: {IsUnassignable ?? false}, PortMappings: {portMappings}, NetworkConfiguration: {networkConfiguration}, SessionHostSummary: {sessionHostSummary}, VmMonitoringOutput: {VmMonitoringOutputId}, ToSViolationRating: {ToSViolationRating}";
            }

            return this.ToJsonString();
        }
    }

    public class HeartbeatRequest
    {
        public string VmId { get; set; }

        public VmAgentInfo VmAgentInfo { get; set; }
    }

    public class HeartbeatResponse
    {
        public bool IsError { get; set; }

        public string ErrorMessage { get; set; }
    }
}
