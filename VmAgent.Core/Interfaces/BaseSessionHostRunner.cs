// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.ContainerEngines
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Gaming.VmAgent.Model;
    using Core.Interfaces;

    using Microsoft.Azure.Gaming.AgentInterfaces;
    using Microsoft.Azure.Gaming.VmAgent.Core;
    using System.IO;
    using System;

    public abstract class BaseSessionHostRunner : ISessionHostRunner
    {
        protected readonly MultiLogger _logger;

        protected readonly VmConfiguration _vmConfiguration;

        protected readonly ISystemOperations _systemOperations;

        protected BaseSessionHostRunner(
            VmConfiguration vmConfiguration,
            MultiLogger logger,
            ISystemOperations systemOperations)
        {
            _logger = logger;
            _vmConfiguration = vmConfiguration;
            _systemOperations = systemOperations;
        }

        abstract public Task CollectLogs(string id, string logsFolder, string dumpsFolderVm, ISessionHostManager sessionHostManager);

        abstract public Task<SessionHostInfo> CreateAndStart(int instanceNumber, GameResourceDetails gameResourceDetails, ISessionHostManager sessionHostManager);

        abstract public Task DeleteResources(SessionHostsStartInfo sessionHostsStartInfo);

        abstract public string GetVmAgentIpAddress();

        abstract public Task<IEnumerable<string>> List();

        abstract public Task RetrieveResources(SessionHostsStartInfo sessionHostsStartInfo);

        abstract public Task<bool> TryDelete(string id);

        abstract public Task WaitOnServerExit(string containerId);
    }
}
