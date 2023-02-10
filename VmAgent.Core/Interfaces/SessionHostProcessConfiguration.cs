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
            : base(vmConfiguration, logger, systemOperations, sessionHostsStartInfo)
        {
        }

        protected override string GetGsdkConfigFilePath(int instanceNumber)
        {
            return Path.Combine(
                VmConfiguration.GetConfigRootFolderForSessionHost(instanceNumber),
                VmDirectories.GsdkConfigFilename);
        }

        protected override string GetLogFolder(string logFolderId)
        {
            return Path.Combine(VmConfiguration.VmDirectories.GameLogsRootFolderVm, logFolderId);
        }

        protected override string GetSharedContentFolder()
        {
            return VmConfiguration.VmDirectories.GameSharedContentFolderVm;
        }

        protected override string GetCertificateFolder()
        {
            return VmConfiguration.VmDirectories.CertificateRootFolderVm;
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


        public override IList<PortMapping> GetPortMappings(int instanceNumber)
        {
            if (_sessionHostsStartInfo.PortMappingsList != null && _sessionHostsStartInfo.PortMappingsList.Count > 0)
            {
                List<PortMapping> result = _sessionHostsStartInfo.PortMappingsList[instanceNumber].Select(portMapping => new PortMapping(portMapping)).ToList();
                result.ForEach(portMapping => portMapping.GamePort.Number = portMapping.NodePort);
                return result;
            }

            return null;
        }

        protected override string GetVmAgentIpAddressInternal()
        {
            return "127.0.0.1";
        }
    }
}
