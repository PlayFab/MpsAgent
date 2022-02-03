// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AgentInterfaces.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Azure.Gaming.AgentInterfaces;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VmAgentInfoTests
    {
        /// <summary>
        /// Tests the log string for VmAgentInfo when there are a lot of sessions. It shouldn't use the default json serialization.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void LogStringWithLotOfSessions()
        {
            VmAgentInfo agentInfo = new VmAgentInfo()
            {
                VmState = VmState.Assigned,
                SessionHostHeartbeatMap = Enumerable.Range(0, 100)
                    .Select(x => new KeyValuePair<string, SessionHostHeartbeatInfo>(x.ToString(), new SessionHostHeartbeatInfo()))
                    .ToDictionary(x => x.Key, x => x.Value)
            };

            string logString = agentInfo.ToLogString();
            Assert.IsTrue(logString.IndexOf("SessionHostSummary", StringComparison.OrdinalIgnoreCase) > -1);
            Assert.IsFalse(logString.IndexOf("SessionHostHeartbeatMap", StringComparison.OrdinalIgnoreCase) > -1);
        }

        /// <summary>
        /// Test to ensure new properties are added to ToLogString.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void KnownProperties()
        {
            IReadOnlyList<string> knownProperties = new List<string>
            {
                nameof(VmAgentInfo.SessionHostHeartbeatMap),
                nameof(VmAgentInfo.MaintenanceSchedule),
                nameof(VmAgentInfo.VmState),
                nameof(VmAgentInfo.AgentProcessGuid),
                nameof(VmAgentInfo.AssignmentId),
                nameof(VmAgentInfo.SequenceNumber),
                nameof(VmAgentInfo.IsUnassignable),
                nameof(VmAgentInfo.NetworkConfiguration),
                nameof(VmAgentInfo.VmMonitoringOutputId)
            };

            HashSet<string> propertyNames =
                new HashSet<string>(typeof(VmAgentInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => x.Name));
            IReadOnlyList<string> newProperties = propertyNames.Except(knownProperties).ToList();
            Assert.IsFalse(newProperties.Any(), $"Please add new properties {string.Join(", ", newProperties)} to the list above and to the ToLogString method.");
        }

        /// <summary>
        /// Test to ensure enum order doesn't change when a new VmState is added. Control plane relies on this enum to determine the order healthyVmsEligibleToRelease.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void VmStateEnumOrder()
        {
            var vmStatesEnumIntCast = new Dictionary<VmState, int>
            {
                {VmState.Unknown, 0},
                {VmState.Unassigned, 1},
                {VmState.Assigned, 2},
                {VmState.Propping, 3},
                {VmState.ProppingFailed, 4},
                {VmState.ProppingCompleted, 5},
                {VmState.ServerStartFailed, 6},
                {VmState.StartServersFailed, 7},
                {VmState.PartiallyRunning, 8},
                {VmState.Running, 9},
                {VmState.PendingResourceCleanup, 10},
                {VmState.ServersRemoved, 12},
                {VmState.TooManyServerRestarts, 13}
            };
            foreach (VmState vmState in vmStatesEnumIntCast.Keys)
            {
                Assert.AreEqual((int)vmState, vmStatesEnumIntCast[vmState]);
            }
        }
    }
}
