// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using AgentInterfaces;
    using VmAgent.Core.Interfaces;
    using VmAgent.Model;

    public class NoOpSessionHostManager : ISessionHostManager
    {
        private ConcurrentDictionary<string, SessionHostInfo> SessionHostsMap = new ConcurrentDictionary<string, SessionHostInfo>();
        
        public bool LinuxContainersOnWindows
        {
            get
            {
                return Globals.GameServerEnvironment == GameServerEnvironment.Linux &&
                       RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            }
        }

        public int StateChangeSequenceNumber { get; }

        public VmAgentSettings VmAgentSettings { get; } = new VmAgentSettings() 
        { 
            EnableCrashDumpProcessing = false,
            EnableTelemetry = false,
        };

        public void Assign(SessionHostsStartInfo request)
        {
        }

        public bool TrySetGoalStateInfo(string sessionHostId, SessionHostGoalStateInfo goalStateInfo)
        {
            return false;
        }

        public (bool sessionHostExists, SessionHostHeartbeatInfo response) TryProcessHeartbeat(string sessionHostId, SessionHostHeartbeatInfo heartbeatRequest)
        {
            return (false, heartbeatRequest);
        }

        public void UpdateStateForAssignment(VmState vmState, ResourceRetrievalResult? assetRetrievalResult = null,
            ResourceRetrievalResult? imageRetrievalResult = null, bool forceSave = false)
        {
        }

        public SessionHostInfo AddNewSessionHost(string sessionHostId, string assignmentId, int instanceNumber, string logFolderId,
            SessionHostType type = SessionHostType.Container)
        {
            var sessionHostInfo = new SessionHostInfo(sessionHostId, assignmentId, instanceNumber, logFolderId, type)
                {
                    SessionHostHeartbeatRequest = new SessionHostHeartbeatInfo()
                    {
                        AssignmentId = assignmentId,
                        CurrentGameState = SessionHostStatus.PendingHeartbeat,
                        LastStateTransitionTimeUtc = DateTime.UtcNow
                    }
                };
            SessionHostsMap.TryAdd(sessionHostId, sessionHostInfo);
            return sessionHostInfo;
        }

        public void UpdateSessionHostTypeSpecificId(string sessionHostId, string typeSpecificId)
        {
            SessionHostsMap.TryGetValue(sessionHostId, out SessionHostInfo sessionHostInfo);
            sessionHostInfo.TypeSpecificId = typeSpecificId;
        }

        public void RemoveSessionHost(string sessionHostId)
        {
        }

        public AssignmentData GetAssignmentData()
        {
            return null;
        }

        public void ClearSecrets()
        {
        }

        public GameResourceDetails GetGameResourceDetails()
        {
            return new GameResourceDetails();
        }

        public VmState SetPendingUnassignment(string assignmentId)
        {
            return VmState.Unassigned;
        }

        public bool IsPendingUnassignment()
        {
            return false;
        }

        public void CompleteUnassignment()
        {
        }

        public SessionHost AllocateSessionHost(string assignmentId, SessionConfig sessionConfig)
        {
            return new SessionHost();
        }

        public List<SessionHost> ListAllocatedSessions()
        {
            return new List<SessionHost>() { new SessionHost() };
        }

        public IEnumerable<KeyValuePair<string, SessionHostInfo>> GetExpiredTerminatedSessions()
        {
            return Enumerable.Empty<KeyValuePair<string, SessionHostInfo>>();
        }

        public IEnumerable<KeyValuePair<string, SessionHostInfo>> GetSessionHosts()
        {
            return Enumerable.Empty<KeyValuePair<string, SessionHostInfo>>();
        }

        public void EvaluateSessionHostStateDuration()
        {
        }

        public VmState GetVmState()
        {
            return VmState.Assigned;
        }

        public bool IsAssigned()
        {
            return true;
        }

        public void SetStartupScriptExecutionComplete()
        {
        }

        public bool IsStartupScriptExecutionComplete()
        {
            return true;
        }

        public bool IsUnassignable()
        {
            return true;
        }

        public void SetCrashDumpState(string sessionHostId, CrashDumpState crashDumpState)
        {

        }

        public void SetProfilingOutputFlag(string sessionHostId)
        {

        }

        public string GetLogFolderForSessionHostId(string sessionHostId)
        {
            return "";
        }

        public string GetTypeSpecificIdForSessionHost(string sessionHostId)
        {
            return "";
        }
    }
}
