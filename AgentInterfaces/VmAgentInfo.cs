// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;

    [ProtoContract]
    public class VmAgentInfo
    {
        [ProtoMember(1)]
        public VmState VmState { get; set; }

        [ProtoMember(2)]
        public string AssignmentId { get; set; }

        [ProtoMember(3)]
        public IDictionary<string, SessionHostHeartbeatInfo> SessionHostHeartbeatMap { get; set; }

        [ProtoMember(4)]
        public MaintenanceSchedule MaintenanceSchedule { get; set; }

        /// <summary>
        /// Sequence number of heartbeat. Newer updates from the same AgentSessionId should have a greater number.
        /// </summary>
        [ProtoMember(5)]
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Session id of Agent.  This is reset when the vm agent process restarts.
        /// </summary>
        [ProtoMember(6)]
        public Guid AgentProcessGuid { get; set; }

        /// <summary>
        /// Nullable boolean that represents if VM is unassignable or not.
        /// </summary>
        [ProtoMember(7)]
        public bool? IsUnassignable { get; set; }

        /// <summary>
        /// The network configuration of the agent, describing the endpoints available on the VM.
        /// </summary>
        [ProtoMember(8)]
        public VmNetworkConfiguration NetworkConfiguration { get; set; }

        /// <summary>
        /// Monitoring work product for this VM
        /// </summary>
        [ProtoMember(9)]
        public string VmMonitoringOutputId { get; set; }

        public string ToLogString()
        {
            if (SessionHostHeartbeatMap?.Count > 10)
            {
                string maintenanceSchedule = MaintenanceSchedule?.ToJsonString() ?? string.Empty;
                string networkConfiguration = NetworkConfiguration?.ToJsonString() ?? string.Empty;
                string sessionHostSummary = SessionHostHeartbeatMap.Values.GroupBy(x => x.CurrentGameState).ToDictionary(y => y.Key, y => y.Count()).ToJsonString();
                return
                    $"VmState: {VmState}, AssignmentId: {AssignmentId ?? string.Empty}, AgentProcessGuid : {AgentProcessGuid}, SequenceNumber {SequenceNumber}, MaintenanceSchedule : {maintenanceSchedule}, IsUnassignable: {IsUnassignable ?? false}, NetworkConfiguration: {networkConfiguration}, SessionHostSummary: {sessionHostSummary}, VmMonitoringOutput: {VmMonitoringOutputId}";
            }

            return this.ToJsonString();
        }
    }

    [ProtoContract]
    public class HeartbeatRequest
    {
        [ProtoMember(1)]
        public string VmId { get; set; }

        [ProtoMember(2)]
        public VmAgentInfo VmAgentInfo { get; set; }
    }

    [ProtoContract]
    public class HeartbeatResponse
    {
        [ProtoMember(1)]
        public bool IsError { get; set; }

        [ProtoMember(2)]
        public string ErrorMessage { get; set; }
    }
}
