// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using AgentInterfaces;

    public class AssignmentOverallData
    {
        public AssignmentData AssignmentData { get; set; }

        public SessionHostsStartInfo GameResourceDetails { get; set; }
    }
}
