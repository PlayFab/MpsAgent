// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System.Collections.Generic;
    using AgentInterfaces;
    using Model;

    // Created to simplify Dependency injection.
    public interface ISessionHostManager
    {
        bool LinuxContainersOnWindows { get; }
        int StateChangeSequenceNumber { get; }

        VmAgentSettings VmAgentSettings { get; }

        void Assign(SessionHostsStartInfo request);

        bool TrySetGoalStateInfo(string sessionHostId, SessionHostGoalStateInfo goalStateInfo);

        (bool sessionHostExists, SessionHostHeartbeatInfo response) TryProcessHeartbeat(
            string sessionHostId,
            SessionHostHeartbeatInfo heartbeatRequest);

        void UpdateStateForAssignment(
            VmState vmState,
            ResourceRetrievalResult? assetRetrievalResult = null,
            ResourceRetrievalResult? imageRetrievalResult = null,
            bool forceSave = false);

        SessionHostInfo AddNewSessionHost(string sessionHostId, string assignmentId, int containerInstanceNumber, string logFolderId, SessionHostType type = SessionHostType.Container);

        public IReadOnlyList<VmCondition> GetVmConditions();
        
        void UpdateSessionHostTypeSpecificId(string sessionHostId, string typeSpecificId);

        void RemoveSessionHost(string sessionHostId);

        AssignmentData GetAssignmentData();

        void ClearSecrets();

        GameResourceDetails GetGameResourceDetails();

        VmState SetPendingUnassignment(string assignmentId);

        bool IsPendingUnassignment();

        void CompleteUnassignment();

        SessionHost AllocateSessionHost(string assignmentId, SessionConfig sessionConfig);

        /// <summary>
        /// List all session hosts
        /// </summary>
        /// <returns>List of all session hosts</returns>
        List<SessionHost> ListAllocatedSessions();

        IEnumerable<KeyValuePair<string, SessionHostInfo>> GetExpiredTerminatedSessions();

        IEnumerable<KeyValuePair<string, SessionHostInfo>> GetSessionHosts();

        void EvaluateSessionHostStateDuration();

        VmState GetVmState();

        bool IsAssigned();

        void SetStartupScriptExecutionComplete(VmStartupScriptCompletionResult result);

        bool IsStartupScriptExecutionComplete();

        bool IsUnassignable();

        void SetCrashDumpState(string sessionHostId, CrashDumpState crashDumpState);

        void SetProfilingOutputFlag(string sessionHostId);

        string GetLogFolderForSessionHostId(string sessionHostId);

        string GetTypeSpecificIdForSessionHost(string sessionHostId);

        void MarkForMaintenance(MaintenanceSchedule schedule);

        bool IsMarkedForMaintenance();
    }
    
    public enum VmStartupScriptCompletionResult
    {
        Success,
        Failure,
        NoNeedToRun
    }
}
