// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class AssignmentData
    {
        public ConcurrentDictionary<string, SessionHostHeartbeatInfo> SessionHostHeartbeatMap { get; set; }

        public VmState VmState { get; set; }

        // Essentially identifies titleId, deploymentId, region.
        public string AssignmentId { get; set; }

        public ResourceRetrievalResult AssetRetrievalResult { get; set; }

        public ResourceRetrievalResult ImageRetrievalResult { get; set; }
    }
}
