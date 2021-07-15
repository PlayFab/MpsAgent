// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System.Collections.Generic;
    using AgentInterfaces;

    public interface ISessionHostConfiguration
    {
        void Create(int instanceNumber, string sessionHostUniqueId, string agentEndpoint, VmConfiguration vmConfiguration, string logFoldeId);

        IDictionary<string, string> GetEnvironmentVariablesForSessionHost(int instanceNumber, string logFolderId, VmAgentSettings agentSettings);

        IList<PortMapping> GetPortMappings(int instanceNumber);
    }
}
