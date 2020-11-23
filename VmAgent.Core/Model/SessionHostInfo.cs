// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System;
    using AgentInterfaces;

    public class SessionHostInfo
    {
        public SessionHostInfo(string sessionHostId, string assignmentId, int instanceNumber, string logFolderId, SessionHostType sessionHostType = SessionHostType.Container)
        {
            UniqueId = sessionHostId;
            AssignmentId = assignmentId;
            InstanceNumber = instanceNumber;
            LogFolderId = logFolderId;
            Type = sessionHostType;
            ReachedStandingBy = false;

            // This can be updated later if necessary.
            TypeSpecificId = UniqueId;
        }

        /// <summary>
        /// An identifier that depends on the type of session host. For containers, this is same as the UniqueId (which is globally unique and never re-used).
        /// For processes, this is the process Id (.ToString()), and the uniqueId is just a generated GUID. We need the uniqueId since that's used by ControlPlane.
        /// We need this id to track processes within the VM.
        /// </summary>
        public string TypeSpecificId { get; set; }

        public SessionHostType Type { get; set; }

        public SessionHostHeartbeatInfo SessionHostHeartbeatRequest { get; set; }

        public SessionHostGoalStateInfo GoalStateInfo { get; set; }

        public DateTime GoalStateRequestedTimestamp { get; set; }

        public string AssignmentId { get; set; }

        public string UniqueId { get; set; }

        public int InstanceNumber { get; set; }

        public bool ReachedStandingBy { get; set; }

        /// <summary>
        /// Guid that specifies the subfolder under C:/Logs where this session is logging to
        /// </summary>
        public string LogFolderId { get; set; }
    }
}
