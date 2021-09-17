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

        public SessionHostContainerConfiguration(
            VmConfiguration vmConfiguration,
            MultiLogger logger,
            Interfaces.ISystemOperations systemOperations,
            IDockerClient dockerClient,
            SessionHostsStartInfo sessionHostsStartInfo) : base(vmConfiguration, logger, systemOperations, sessionHostsStartInfo)
        {
            _dockerClient = dockerClient;
        }
        
        public SessionHostContainerConfiguration(
            VmConfiguration vmConfiguration,
            MultiLogger logger,
            Interfaces.ISystemOperations systemOperations,
            IDockerClient dockerClient,
            SessionHostsStartInfo sessionHostsStartInfo,
            ISessionHostManager sessionHostManager
            ) : base(vmConfiguration, logger, systemOperations, sessionHostsStartInfo, sessionHostManager)
        {
            _dockerClient = dockerClient;
        }

        protected override string GetGsdkConfigFilePath(string assignmentId, int instanceNumber)
        {
            return VmConfiguration.VmDirectories.GsdkConfigFilePathContainer;
        }

        protected override string GetCertificatesPath(string assignmentId)
        {
            return VmConfiguration.VmDirectories.CertificateRootFolderContainer;
        }

        protected override string GetSharedContentFolderPath()
        {
            return VmConfiguration.VmDirectories.GameSharedContentFolderContainer;
        }

        protected override string GetLogFolder(string logFolderId, VmConfiguration vmConfiguration)
        {
            // The VM host folder corresponding to the logFolderId gets mounted under this path for each container.
            // So the logFolderId itself isn't of much significance within the container.
            if (_sessionHostManager != null && _sessionHostManager.LinuxContainersOnWindows)
            {
                return vmConfiguration.VmDirectories.GameLogsRootFolderContainer + Path.AltDirectorySeparatorChar;
            }
            else
            {
                return vmConfiguration.VmDirectories.GameLogsRootFolderContainer + Path.DirectorySeparatorChar;
            }
        }

        protected override string GetSharedContentFolder(VmConfiguration vmConfiguration)
        {
            return vmConfiguration.VmDirectories.GameSharedContentFolderContainer;
        }

        protected override string GetCertificateFolder(VmConfiguration vmConfiguration)
        {
            return vmConfiguration.VmDirectories.CertificateRootFolderContainer;
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

        /// <inheritdoc/>
        protected override IEnumerable<GamePort> GetGamePortConfiguration(int instanceNumber)
        {
            IList<PortMapping> portMappings = GetPortMappings(instanceNumber);
            return portMappings.Select(port => new GamePort()
            {
                Name = port.GamePort.Name,
                ServerListeningPort = port.GamePort.Number,
                ClientConnectionPort = port.PublicPort
            });
        }

        protected override IDictionary<string, string> GetPortMappingsInternal(List<PortMapping> portMappings)
        {
            return portMappings?.ToDictionary(x => x.GamePort.Name, x => x.GamePort.Number.ToString());
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
