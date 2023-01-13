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
        /// The network configuration of the agent, describing the endpoints available on the VM.
        /// </summary>
        public VmNetworkConfiguration NetworkConfiguration { get; set; }

        /// <summary>
        /// Monitoring work product for this VM
        /// </summary>
        public string VmMonitoringOutputId { get; set; }

        public ToSViolationRating ToSViolationRating { get; set; }

        public IReadOnlyList<VmCondition> VmConditions => _vmConditions?.AsReadOnly();
        private List<VmCondition> _vmConditions { get; set; }

        /// <summary>
        /// used for single thread access when adding a new VmCondition
        /// </summary>
        private object lockerObject = new object();
        
        /// <summary>
        /// Adds a new VmCondition to the underlying list
        /// If a VmCondition with the same condition string already exists, it is replaced
        /// </summary>
        /// <param name="newVmCondition"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddVmCondition(VmCondition newVmCondition)
        {
            lock (lockerObject)
            {
                if (newVmCondition == null)
                {
                    throw new ArgumentNullException(nameof(newVmCondition));
                }

                // initialize the list if null
                _vmConditions ??= new List<VmCondition>();

                // check if condition already exists
                if (_vmConditions.Count(x => x.Condition == newVmCondition.Condition) > 0)
                {
                    VmCondition existingCondition = _vmConditions.First(x => x.Condition == newVmCondition.Condition);
                    existingCondition.Reason = newVmCondition.Reason;
                    existingCondition.When = newVmCondition.When;
                }
                else
                {
                    _vmConditions.Add(newVmCondition);
                }
            }
        }
        
        public string ToLogString()
        {
            if (SessionHostHeartbeatMap?.Count > 10)
            {
                string maintenanceSchedule = MaintenanceSchedule?.ToJsonString() ?? string.Empty;
                string networkConfiguration = NetworkConfiguration?.ToJsonString() ?? string.Empty;
                string sessionHostSummary = SessionHostHeartbeatMap.Values.GroupBy(x => x.CurrentGameState).ToDictionary(y => y.Key, y => y.Count()).ToJsonString();
                string vmConditions = VmConditions?.ToJsonString() ?? string.Empty;
                return
                    $"VmState: {VmState}, AssignmentId: {AssignmentId ?? string.Empty}, AgentProcessGuid : {AgentProcessGuid}, SequenceNumber {SequenceNumber}, MaintenanceSchedule : {maintenanceSchedule}, IsUnassignable: {IsUnassignable ?? false}, NetworkConfiguration: {networkConfiguration}, SessionHostSummary: {sessionHostSummary}, VmMonitoringOutput: {VmMonitoringOutputId}, ToSViolationRating: {ToSViolationRating}, VmConditions: {vmConditions}";
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
