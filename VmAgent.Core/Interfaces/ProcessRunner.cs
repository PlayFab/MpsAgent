// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AgentInterfaces;
    using Microsoft.Azure.Gaming.VmAgent.ContainerEngines;
    using Model;
    using VmAgent.Extensions;

    public class ProcessRunner : BaseSessionHostRunner
    {
        private readonly IProcessWrapper _processWrapper;

        public ProcessRunner(
            VmConfiguration vmConfiguration,
            MultiLogger logger,
            ISystemOperations systemOperations,
            IProcessWrapper processWrapper = null)
            : base (vmConfiguration, logger, systemOperations)
        {
            _processWrapper = processWrapper;
        }

        public override Task<SessionHostInfo> CreateAndStart(int instanceNumber, GameResourceDetails gameResourceDetails, ISessionHostManager sessionHostManager)
        {
            SessionHostsStartInfo sessionHostStartInfo = gameResourceDetails.SessionHostsStartInfo;
            string sessionHostUniqueId = Guid.NewGuid().ToString("D");
            string logFolderPathOnVm = Path.Combine(_vmConfiguration.VmDirectories.GameLogsRootFolderVm, sessionHostUniqueId);
            _systemOperations.CreateDirectory(logFolderPathOnVm);

            // Create the dumps folder as a subfolder of the logs folder
            string dumpFolderPathOnVm = Path.Combine(logFolderPathOnVm, VmDirectories.GameDumpsFolderName);
            _systemOperations.CreateDirectory(dumpFolderPathOnVm);

            ISessionHostConfiguration sessionHostConfiguration = new SessionHostProcessConfiguration(_vmConfiguration, _logger, _systemOperations, sessionHostStartInfo);
            string configFolderPathOnVm = _vmConfiguration.GetConfigRootFolderForSessionHost(instanceNumber);
            _systemOperations.CreateDirectory(configFolderPathOnVm);

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            (string executableFileName, string arguments) = GetExecutableAndArguments(sessionHostStartInfo, instanceNumber);
            processStartInfo.FileName = executableFileName;
            processStartInfo.Arguments = arguments;
            processStartInfo.WorkingDirectory = sessionHostStartInfo.GameWorkingDirectory ?? Path.GetDirectoryName(executableFileName);
            processStartInfo.Environment.AddRange(sessionHostConfiguration.GetEnvironmentVariablesForSessionHost(instanceNumber, sessionHostUniqueId, sessionHostManager.VmAgentSettings));

            _logger.LogInformation($"Starting process for session host with instance number {instanceNumber} and process info: FileName - {executableFileName}, Args - {arguments}.");

            SessionHostInfo sessionHost = sessionHostManager.AddNewSessionHost(sessionHostUniqueId, sessionHostStartInfo.AssignmentId, instanceNumber, sessionHostUniqueId, SessionHostType.Process);
            sessionHostConfiguration.Create(instanceNumber, sessionHostUniqueId, GetVmAgentIpAddress(), _vmConfiguration, sessionHostUniqueId);

            try
            {
                string processId = _processWrapper.Start(processStartInfo).ToString();
                sessionHostManager.UpdateSessionHostTypeSpecificId(sessionHostUniqueId, processId);
                _logger.LogInformation($"Started process for session host. Instance Number: {instanceNumber}, UniqueId: {sessionHostUniqueId}, ProcessId: {processId}");
            }
            catch (Exception exception)
            {
                _logger.LogException($"Failed to start process based host with instance number {instanceNumber}", exception);
                sessionHostManager.RemoveSessionHost(sessionHostUniqueId);
                sessionHost = null;
            }

            return Task.FromResult(sessionHost);
        }

        public override Task CollectLogs(string processId, string logsFolder, string dumpsFolderVm, ISessionHostManager sessionHostManager)
        {
            // The game server is free to read the env variable for log folder and write all output to a file in that folder.
            // If required, we can add action handlers (see SystemOperations.RunProcess for example).
            // However, keeping the file handle around can be tricky.
            DirectoryInfo dumpsDirectory = new DirectoryInfo(dumpsFolderVm);
            string dmpFileEnding = $".{processId}.dmp";

            try
            {
                foreach (FileInfo dumpFile in dumpsDirectory.EnumerateFiles())
                {
                    if (dumpFile.Name.EndsWith(dmpFileEnding))
                    {
                        string destination = Path.Combine(logsFolder, VmDirectories.GameDumpsFolderName, dumpFile.Name);
                        _logger.LogVerbose($"Moving dump file {dumpFile.Name} to {destination}.");
                        _systemOperations.FileMoveWithOverwrite(dumpFile.FullName, destination);
                    }
                }
            }
            catch (DirectoryNotFoundException) { }

            return Task.CompletedTask;
        }

        private (string, string) GetExecutableAndArguments(SessionHostsStartInfo sessionHostsStartInfo, int instanceNumber)
        {
            string[] parts = sessionHostsStartInfo.StartGameCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string localPathForAsset0 = sessionHostsStartInfo.UseReadOnlyAssets ? _vmConfiguration.GetAssetExtractionFolderPathForSessionHost(0, 0) :
                _vmConfiguration.GetAssetExtractionFolderPathForSessionHost(instanceNumber, 0);

            // Replacing the mount path is for back compat when we didn't have first class support for process based servers
            // (and were based off of the parameters for containers).
            string executablePath = sessionHostsStartInfo.AssetDetails[0].MountPath?.Length > 0
                ? parts[0].Replace(sessionHostsStartInfo.AssetDetails[0].MountPath, $"{localPathForAsset0}\\")
                : Path.Combine(localPathForAsset0, parts[0]);
            string args = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : string.Empty;
            return (executablePath, args);
        }

        public override Task<bool> TryDelete(string id)
        {
            // TODO: We do not have a good way of cross-platform support to get all children of that process and then kill all of them.
            // We recommend that developers write a bootstrapper that waits for all underlying processes to exit and use the bootstrapper as the startup executable.
            try
            {
                _processWrapper.Kill(int.Parse(id));
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }

            return Task.FromResult(false);
        }

        public override Task DeleteResources(SessionHostsStartInfo sessionHostsStartInfo)
        {
            // no-op
            return Task.CompletedTask;
        }

        public override Task RetrieveResources(SessionHostsStartInfo sessionHostsStartInfo)
        {
            // no-op
            return Task.CompletedTask;
        }

        public override Task<IEnumerable<string>> List()
        {
            return Task.FromResult(_processWrapper.List().Select(x => x.ToString()));
        }

        public override Task WaitOnServerExit(string containerId)
        {
            _processWrapper.WaitForProcessExit(int.Parse(containerId));
            return Task.CompletedTask;
        }

        public override string GetVmAgentIpAddress()
        {
            return "127.0.0.1";
        }
    }
}
