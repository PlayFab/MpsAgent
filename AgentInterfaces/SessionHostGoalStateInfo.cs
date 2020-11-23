// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    public class SessionHostGoalStateInfo
    {
        public SessionHostStatus GoalState { get; set; }

        public SessionConfig SessionConfig { get; set; }
    }
}
