// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System.Collections.Generic;

    /// <summary>
    ///     Models currentRoleInstance object in legacy ServiceDefinition.json file
    /// </summary>
    public class CurrentRoleInstance
    {
        public string Id { get; set; }

        public List<RoleInstanceEndpoint> RoleInstanceEndpoints { get; set; }
    }

    public class RoleInstanceEndpoint
    {
        public string IpEndPoint { get; set; }

        public string Name { get; set; }

        public string Protocol { get; set; }

        public string PublicIpEndPoint { get; set; }

        public string InternalPort { get; set; }

        public string ExternalPort { get; set; }
    }
}
