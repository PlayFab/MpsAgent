// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using AgentInterfaces;

    public class VmPersistedState: IPersistedState
    {
        public VmState VmState { get; set; } = VmState.Unassigned;

        public ConcurrentDictionary<string, SessionHostInfo> SessionHostsMap = new ConcurrentDictionary<string, SessionHostInfo>();

        public ResourceRetrievalResult AssetRetrievalResult { get; set; }

        public ResourceRetrievalResult ImageRetrievalResult { get; set; }

        public SessionHostsStartInfo GameResourceDetails { get; set; }

        /// <summary>
        /// Maintenance schedule for this VM, if any.
        /// </summary>
        public MaintenanceSchedule MaintenanceSchedule { get; set; }

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

        public string ToRedactedString()
        {
            IDictionary<string, SessionHostInfo> t = SessionHostsMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return JsonSerializer.Serialize(
                new {
                    this.VmState,
                    // create a new SessionHostsMap that doesn't have SessionHostHeartbeatInfo or SessionHostGoalStateInfo
                    SessionHostsMap = SessionHostsMap.ToDictionary(
                        (KeyValuePair<string, SessionHostInfo> kvp) => { return kvp.Key; },
                        (KeyValuePair<string, SessionHostInfo> kvp) =>
                        {
                            SessionHostInfo sessionHostInfo = kvp.Value;
                            return new SessionHostInfo(
                                sessionHostInfo.UniqueId,
                                sessionHostInfo.AssignmentId,
                                sessionHostInfo.InstanceNumber,
                                sessionHostInfo.LogFolderId,
                                sessionHostInfo.Type);
                        }),
                    this.AssetRetrievalResult,
                    this.ImageRetrievalResult,
                    GameResourceDetails = new SessionHostsStartInfo()
                    {
                        AssetDetails = GameResourceDetails.AssetDetails,
                        AssignmentId = GameResourceDetails.AssignmentId,
                        Count = GameResourceDetails.Count,
                        DeploymentMetadata = null,
                        DownloadAssetsInStreaming = GameResourceDetails.DownloadAssetsInStreaming,
                        FQDN = GameResourceDetails.FQDN,

                    },
                    this.MaintenanceSchedule,
                    this.IsSessionHostStartupScriptExecutionComplete,
                });
        }
    }
}
