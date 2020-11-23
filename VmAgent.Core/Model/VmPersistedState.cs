// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System.Collections.Concurrent;
    using System.Linq;
    using AgentInterfaces;

    public class VmPersistedState
    {
        public VmState VmState { get; set; } = VmState.Unassigned;

        public ConcurrentDictionary<string, SessionHostInfo> SessionHostsMap = new ConcurrentDictionary<string, SessionHostInfo>();

        public ResourceRetrievalResult AssetRetrievalResult { get; set; }

        public ResourceRetrievalResult ImageRetrievalResult { get; set; }

        public SessionHostsStartInfo GameResourceDetails { get; set; }

        /// <summary>
        /// Whether the start up script (at VM) level has been executed. The script should be run before starting session hosts.
        /// </summary>
        public bool IsSessionHostStartupScriptExecutionComplete { get; set; }

        public AssignmentData ToAssignmentData()
        {
            if (GameResourceDetails == null)
            {
                return null;
            }

            return new AssignmentData()
            {
                AssetRetrievalResult = AssetRetrievalResult,
                ImageRetrievalResult = ImageRetrievalResult,
                AssignmentId = GameResourceDetails.AssignmentId,
                VmState = VmState,
                SessionHostHeartbeatMap =
                    new ConcurrentDictionary<string, SessionHostHeartbeatInfo>(SessionHostsMap.ToDictionary(x => x.Key,
                        x => x.Value.SessionHostHeartbeatRequest))
            };
        }
    }
}
