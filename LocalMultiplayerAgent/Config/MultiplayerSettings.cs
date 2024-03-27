// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.Config
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AgentInterfaces;
    using Docker.DotNet.Models;
    using VmAgent.Core;

    public class MultiplayerSettings
    {
        private const double BytesPerGib = 1024 * 1024 * 1024;
        private const double UnitsInNano = 1_000_000_000;
        
        public string Region { get; set; }

        public int AgentListeningPort { get; set; }

        public string TitleId { get; set; }

        public Guid BuildId { get; set; }

        public int NumHeartBeatsForActivateResponse { get; set; }

        public int NumHeartBeatsForTerminateResponse { get; set; }

        public int NumHeartBeatsForMaintenanceEventResponse { get; set; }

        public bool RunContainer { get; set; }

        public string OutputFolder { get; set; }

        public GameCertificateDetails[] GameCertificateDetails { get; set; }
        public AssetDetail[] AssetDetails { get; set; }

        public List<List<PortMapping>> PortMappingsList { get; set; }

        public ContainerStartParameters ContainerStartParameters { get; set; }

        public ProcessStartParameters ProcessStartParameters { get; set; }

        public SessionConfig SessionConfig { get; set; }

        public bool ForcePullFromAcrOnLinuxContainersOnWindows { get; set; }

        public IDictionary<string, string> DeploymentMetadata { get; set; }

        public string MaintenanceEventType { get; set; }

        public string MaintenanceEventStatus { get; set; }

        public string MaintenanceEventSource { get; set; }

        public SessionHostsStartInfo ToSessionHostsStartInfo()
        {
            // Clear mount path in process based, otherwise, agent will kick into back-compat mode and try to strip the
            // mount path from the start game command before running 
            AssetDetail[] assetDetails = AssetDetails?.Select(x => new AssetDetail()
            {
                MountPath = RunContainer ? x.MountPath : null,
                LocalFilePath = x.LocalFilePath
            }).ToArray();

            var startInfo = new SessionHostsStartInfo
            {
                AssignmentId = $"{VmConfiguration.GetGuidFromTitleId(TitleIdUlong)}:{BuildId}:{Region}",
                SessionHostType = RunContainer ? SessionHostType.Container : SessionHostType.Process,
                PublicIpV4Address = "127.0.0.1",
                FQDN = "localhost",
                HostConfigOverrides = GetHostConfig(),
                ImageDetails = RunContainer ? ContainerStartParameters.ImageDetails : null,
                AssetDetails = assetDetails,
                StartGameCommand = RunContainer ? ContainerStartParameters.StartGameCommand : ProcessStartParameters.StartGameCommand,
                PortMappingsList = PortMappingsList,
                DeploymentMetadata = DeploymentMetadata
            };

            return startInfo;
        }

        private HostConfig GetHostConfig()
        {
            // More info at https://docs.docker.com/config/containers/resource_constraints/
            if (!RunContainer || ContainerStartParameters.ResourceLimits == null)
                return null;

            return new HostConfig()
            {
                NanoCPUs = (long)(ContainerStartParameters.ResourceLimits.Cpus * UnitsInNano),
                Memory = (long)(ContainerStartParameters.ResourceLimits.MemoryGib * BytesPerGib)
            };
        }

        public void SetDefaultsIfNotSpecified()
        {
            if (string.IsNullOrWhiteSpace(TitleId))
            {
                TitleId = unchecked((ulong)new Random().Next()).ToString("X");
            }

            if (BuildId == Guid.Empty)
            {
                BuildId = Guid.NewGuid();
            }

            if (string.IsNullOrWhiteSpace(OutputFolder))
            {
                string defaultOutputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Console.WriteLine($"No output directory specified, defaulting to agent directory");
                OutputFolder = defaultOutputFolder;
            }

            // on LocalMultiplayerAgent, PublicPort is the same as NodePort
            foreach (var portList in PortMappingsList)
            {
                foreach (var portInfo in portList)
                {
                    if (portInfo.PublicPort == 0)
                    {
                        portInfo.PublicPort = portInfo.NodePort;
                    }
                }
            }
        }

        public ulong TitleIdUlong => ulong.Parse(TitleId, NumberStyles.HexNumber);
    }

    public class GameCertificateDetails
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
