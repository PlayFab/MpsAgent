// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using AgentInterfaces;
    using Core;
    using Newtonsoft.Json;

    /// <summary>
    ///     Models the request/response from the legacy GSDK (v140)
    /// </summary>
    public class LegacyGameInfo
    {
        // Some of the older GSDK extracts the session id from the complete session directory url.
        // So we have this so that legacy GSDK works.
        // DO NOT USE THIS FOR ANYTHING ELSE.
        private static readonly Uri SessionDirectoryBaseUrl = new Uri("https://client-sessiondirectory.xboxlive.com/");

        public LegacyGameInfo()
        {
            ClusterManifest = new Dictionary<string, string>
            {
                {"sessionUrl", string.Empty},
                {"sessionType", string.Empty},
                {"gameVariantId", string.Empty}
            };
        }

        [JsonProperty("newState")]
        public SessionHostStatus NewState { get; set; }

        // Note: The following 4 fields aren't used by our logic yet, keeping them for now but might clean up later
        public string AzureStopping { get; set; }

        [JsonProperty("secureDeviceAddress")]
        public string SecureDeviceAddress { get; set; }

        public string SessionTicket { get; set; }

        public string SessionType { get; set; }

        // Response
        [ReadOnly(true)]
        public string NextBeatMilliseconds { get; set; }

        [ReadOnly(true)]
        public Operation? Operation { get; set; }

        [JsonProperty("clusterManifest")]
        [ReadOnly(true)]
        public IDictionary<string, string> ClusterManifest { get; set; }

        [JsonProperty("sessionHostManifest")]
        public IDictionary<string, string> SessionHostManifest => ClusterManifest;

        public static LegacyGameInfo FromSessionHostHeartbeatInfo(SessionHostHeartbeatInfo heartbeatInfo, string legacyTitleId)
        {
            LegacyGameInfo result = new LegacyGameInfo();
            result.NextBeatMilliseconds = heartbeatInfo.NextHeartbeatIntervalMs.ToString();
            result.Operation = heartbeatInfo.Operation;
            if (heartbeatInfo.SessionConfig != null)
            {
                if (heartbeatInfo.SessionConfig.LegacyAllocationInfo != null)
                {
                    result.ClusterManifest = heartbeatInfo.SessionConfig.LegacyAllocationInfo.ClusterManifest;
                }
                else
                {
                    string sessionIdString = heartbeatInfo.SessionConfig.SessionId.ToString();
                    result.ClusterManifest["sessionId"] = sessionIdString;
                    result.ClusterManifest["sessionUrl"] = new Uri(SessionDirectoryBaseUrl, sessionIdString).AbsoluteUri;
                    result.ClusterManifest["sessionCookie"] = heartbeatInfo.SessionConfig?.SessionCookie;

                    if (ulong.TryParse(legacyTitleId, out ulong titleIdLong) &&
                        LegacyTitleHelper.LegacyTitleMappings.TryGetValue(VmConfiguration.GetGuidFromTitleId(titleIdLong), out LegacyTitleDetails titleDetails))
                    {
                        result.ClusterManifest["gameVariantId"] = titleDetails.VariantId.ToString();
                    }
                }
            }

            return result;
        }

        public SessionHostHeartbeatInfo ToSessionHostHeartbeatInfo()
        {
            return new SessionHostHeartbeatInfo()
            {
                CurrentGameState = NewState,
                SecureDeviceAddress = SecureDeviceAddress
            };
        }
    }
}
