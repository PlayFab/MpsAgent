// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AgentInterfaces;
    using Model;

    public interface ISessionHostRunner
    {
        Task<SessionHostInfo> CreateAndStart(int instanceNumber, GameResourceDetails gameResourceDetails, ISessionHostManager sessionHostManager);

        Task<bool> TryDelete(string id);

        Task DeleteResources(SessionHostsStartInfo sessionHostsStartInfo);

        Task RetrieveResources(SessionHostsStartInfo sessionHostsStartInfo);

        Task<IEnumerable<string>> List();

        string GetVmAgentIpAddress();

        Task WaitOnServerExit(string containerId);

        Task CollectLogs(string id, string logsFolder, ISessionHostManager sessionHostManager);

        Task CreateStartGameExceptionLogs(string logFolder, string exceptionMessage);
    }
}
