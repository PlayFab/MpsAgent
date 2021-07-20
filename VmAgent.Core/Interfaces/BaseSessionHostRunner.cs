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

        abstract public Task CollectLogs(string id, string logsFolder, ISessionHostManager sessionHostManager);

        abstract public Task<SessionHostInfo> CreateAndStart(int instanceNumber, GameResourceDetails gameResourceDetails, ISessionHostManager sessionHostManager);

        abstract public Task DeleteResources(SessionHostsStartInfo sessionHostsStartInfo);

        abstract public string GetVmAgentIpAddress();

        abstract public Task<IEnumerable<string>> List();

        abstract public Task RetrieveResources(SessionHostsStartInfo sessionHostsStartInfo);

        abstract public Task<bool> TryDelete(string id);

        abstract public Task WaitOnServerExit(string containerId);

        protected void ProcessDumps(string id, string logsFolder, ISessionHostManager sessionHostManager)
        {
            if (sessionHostManager.VmAgentSettings.EnableCrashDumpProcessing)
            {
                try
                {
                    string dumpFolder = Path.Combine(logsFolder, VmDirectories.GameDumpsFolderName);
                    bool dumpFound = false;
                    try
                    {
                        if (!_systemOperations.IsDirectoryEmpty(dumpFolder))
                        {
                            dumpFound = true;
                        }
                        else
                        {
                            // If dumps folder is empty, delete it
                            _systemOperations.DeleteDirectoryIfExists(dumpFolder);
                        }
                    }
                    catch (DirectoryNotFoundException) { }

                    if (dumpFound)
                    {
                        bool shouldDeleteDump = sessionHostManager.SignalDumpFoundAndCheckIfThrottled(id);
                        if (shouldDeleteDump)
                        {
                            _systemOperations.DeleteDirectoryIfExists(dumpFolder);
                            _systemOperations.CreateDirectory(dumpFolder);
                            string readmePath = Path.Combine(dumpFolder, "readme.txt");
                            _systemOperations.FileWriteAllText(readmePath, $"The contents of \"{VmDirectories.GameDumpsFolderName}\" have been deleted due to throttling.");
                        }
                    }
                }
                catch (IOException ex)
                {
                    // I think we'd only end up here if a game server spun up a background process that had a lock on some files
                    // in the dumps folder, and then the game server crashed. The background process could theoretically still be
                    // running, which would prevent us from processing the dump files.
                    _logger.LogWarning($"Unable to process dump files on session host {id}: {ex}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected error processing dump files on session host {id}: {ex}");
                }
            }
        }
    }
}
