// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AgentInterfaces;
    using Docker.DotNet;
    using Model;

    public class SessionHostContainerConfiguration : SessionHostConfigurationBase
    {
        public const string DockerNetworkName = "playfab";

        private readonly IDockerClient _dockerClient;

        private readonly bool _isRunningLinuxContainersOnWindows;

        private readonly bool _shouldPublicPortMatchGamePort;

        public SessionHostContainerConfiguration(
            VmConfiguration vmConfiguration,
            MultiLogger logger,
            Interfaces.ISystemOperations systemOperations,
            IDockerClient dockerClient,
            SessionHostsStartInfo sessionHostsStartInfo,
            bool isRunningLinuxContainersOnWindows = false,
            bool shouldPublicPortMatchGamePort = false) : base(vmConfiguration, logger, systemOperations, sessionHostsStartInfo)
        {
            _dockerClient = dockerClient;
            _isRunningLinuxContainersOnWindows = isRunningLinuxContainersOnWindows;
            _shouldPublicPortMatchGamePort = shouldPublicPortMatchGamePort;
        }
        
        protected override string GetGsdkConfigFilePath(int instanceNumber)
        {
            return VmConfiguration.VmDirectories.GsdkConfigFilePathContainer;
        }

        protected override string GetLogFolder(string logFolderId)
        {
            // The VM host folder corresponding to the logFolderId gets mounted under this path for each container.
            // So the logFolderId itself isn't of much significance within the container.
            if (_isRunningLinuxContainersOnWindows)
            {
                return VmConfiguration.VmDirectories.GameLogsRootFolderContainer + Path.AltDirectorySeparatorChar;
            }
            else
            {
                return VmConfiguration.VmDirectories.GameLogsRootFolderContainer + Path.DirectorySeparatorChar;
            }
        }

        protected override string GetSharedContentFolder()
        {
            return VmConfiguration.VmDirectories.GameSharedContentFolderContainer;
        }

        protected override string GetCertificateFolder()
        {
            return VmConfiguration.VmDirectories.CertificateRootFolderContainer;
        }

        /// <summary>
        /// Obtains the port name and values in legacy format. This differs from the implementation in <see cref="SessionHostProcessConfiguration"/>
        /// in the fact that for internal ports, this just uses the port in build configuration as each container can listen on the same port within a VM.
        /// </summary>
        /// <param name="instanceNumber"></param>
        /// <returns></returns>
        protected override Dictionary<string, string> GetLegacyPortMapping(int instanceNumber)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (PortMapping portMapping in _sessionHostsStartInfo.PortMappingsList[instanceNumber])
            {
                // The internal port is just the port number provided by customer as each container has its own port space.
                result[$"Port{portMapping.GamePort.Name}Internal"] = portMapping.GamePort.Number.ToString();
                result[$"Port{portMapping.GamePort.Name}External"] = portMapping.PublicPort.ToString();
            }

            return result;
        }

        protected override int GetLegacyServerListeningPort(PortMapping portMapping)
        {
            return portMapping.GamePort.Number;
        }

        public override IList<PortMapping> GetPortMappings(int instanceNumber)
        {
            if (_sessionHostsStartInfo.PortMappingsList != null && _sessionHostsStartInfo.PortMappingsList.Count > 0)
            {
                List<PortMapping> result = _sessionHostsStartInfo.PortMappingsList[instanceNumber].Select(portMapping => new PortMapping(portMapping)).ToList();
                if (_shouldPublicPortMatchGamePort)
                {
                    result.ForEach(portMapping => portMapping.GamePort.Number = portMapping.PublicPort);
                }

                return result;
            }

            return null;
        }

        protected override string GetVmAgentIpAddressInternal()
        {
            return
                _dockerClient.Networks.ListNetworksAsync()
                    .Result.Single(
                        x => string.Equals(DockerNetworkName, x.Name, StringComparison.OrdinalIgnoreCase))
                    .IPAM.Config.Single().Gateway;
        }
    }
}
