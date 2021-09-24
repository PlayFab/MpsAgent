// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AgentInterfaces;
    using Model;

    public class SessionHostProcessConfiguration : SessionHostConfigurationBase
    {
        public SessionHostProcessConfiguration(VmConfiguration vmConfiguration, MultiLogger logger, ISystemOperations systemOperations, SessionHostsStartInfo sessionHostsStartInfo)
            : base(vmConfiguration, logger, systemOperations, sessionHostsStartInfo, isRunningLinuxContainersOnWindows: false)
        {
        }

        protected override string GetGsdkConfigFilePath(string assignmentId, int instanceNumber)
        {
            return Path.Combine(
                VmConfiguration.GetConfigRootFolderForSessionHost(instanceNumber),
                VmDirectories.GsdkConfigFilename);
        }

        protected override string GetCertificatesPath(string assignmentId)
        {
            return VmConfiguration.VmDirectories.CertificateRootFolderVm;
        }

        protected override string GetSharedContentFolderPath()
        {
            return VmConfiguration.VmDirectories.GameSharedContentFolderVm;
        }

        protected override string GetLogFolder(string logFolderId, VmConfiguration vmConfiguration)
        {
            return Path.Combine(vmConfiguration.VmDirectories.GameLogsRootFolderVm, logFolderId);
        }

        protected override string GetSharedContentFolder(VmConfiguration vmConfiguration)
        {
            return vmConfiguration.VmDirectories.GameSharedContentFolderVm;
        }

        protected override string GetCertificateFolder(VmConfiguration vmConfiguration)
        {
            return vmConfiguration.VmDirectories.CertificateRootFolderVm;
        }

        /// <summary>
        /// Obtains the port name and values in legacy format. This differs from the implementation in <see cref="SessionHostContainerConfiguration"/>
        /// in the fact that for internal ports, this uses the actual port on the VM since each process should listen on a different port on the VM.
        /// </summary>
        /// <param name="instanceNumber"></param>
        /// <returns></returns>
        protected override Dictionary<string, string> GetLegacyPortMapping(int instanceNumber)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (PortMapping portMapping in _sessionHostsStartInfo.PortMappingsList[instanceNumber])
            {
                // The internal port is the port on the VM (each process should use a different VM port).
                result[$"Port{portMapping.GamePort.Name}Internal"] = portMapping.NodePort.ToString();
                result[$"Port{portMapping.GamePort.Name}External"] = portMapping.PublicPort.ToString();
            }

            return result;
        }

        protected override int GetLegacyServerListeningPort(PortMapping portMapping)
        {
            return portMapping.NodePort;
        }

        /// <inheritdoc/>
        protected override IEnumerable<GamePort> GetGamePortConfiguration(int instanceNumber)
        {
            IList<PortMapping> portMappings = GetPortMappings(instanceNumber);
            return portMappings.Select(port => new GamePort()
            {
                Name = port.GamePort.Name,
                ServerListeningPort = port.NodePort,
                ClientConnectionPort = port.PublicPort
            });
        }

        protected override IDictionary<string, string> GetPortMappingsInternal(List<PortMapping> portMappings)
        {
            return portMappings?.ToDictionary(x => x.GamePort.Name, x => x.NodePort.ToString());
        }

        protected override string GetVmAgentIpAddressInternal()
        {
            return "127.0.0.1";
        }
    }
}
