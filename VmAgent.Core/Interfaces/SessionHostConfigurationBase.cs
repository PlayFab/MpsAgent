// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AgentInterfaces;
    using Extensions;
    using Microsoft.Azure.Gaming.VmAgent.Extensions;
    using Microsoft.Extensions.Logging;
    using Model;
    using Newtonsoft.Json;

    public abstract class SessionHostConfigurationBase : ISessionHostConfiguration
    {
        // The server instance number (1 to NumSessionsPerVm) for this session host.
        private const string ServerInstanceNumberEnvVariable = "PF_SERVER_INSTANCE_NUMBER";
        
        // Used by the games to share  user generated content (and other files that are downloaded once, used multiple times).
        private const string SharedContentFolderEnvVariable = "PF_SHARED_CONTENT_FOLDER";

        // Used by the GSDK to find the configuration file
        private const string ConfigFileEnvVariable = "GSDK_CONFIG_FILE";

        // Used by the startup script to install Game Certificates
        private const string CertificateFolderEnvVariable = "CERTIFICATE_FOLDER";

        /// <summary>
        /// An environment variable capturing the logs folder for a game server.
        /// All logs written to this folder will be zipped and uploaded to a storage account and are available for download.
        /// </summary>
        private const string LogsDirectoryEnvVariable = "PF_SERVER_LOG_DIRECTORY";

        /// <summary>
        /// An environment variable capturing the crash dumps folder for a game server.
        /// This folder is a subfolder of PF_SERVER_LOG_DIRECTORY.
        /// </summary>
        private const string DumpsDirectoryEnvVariable = "PF_SERVER_DUMP_DIRECTORY";

        // Not sure if this is needed yet.
        private const string DefaultExePath = @"C:\app\";

        protected VmConfiguration VmConfiguration { get; }

        private readonly ILogger _logger;

        private readonly ISystemOperations _systemOperations;

        protected readonly SessionHostsStartInfo _sessionHostsStartInfo;

        protected abstract string GetGsdkConfigFilePath(string assignmentId, int instanceNumber);

        protected abstract string GetCertificatesPath(string assignmentId);

        protected abstract string GetSharedContentFolderPath();

        protected SessionHostConfigurationBase(VmConfiguration vmConfiguration, MultiLogger logger, ISystemOperations systemOperations, SessionHostsStartInfo sessionHostsStartInfo)
        {
            _logger = logger;
            VmConfiguration = vmConfiguration;
            _systemOperations = systemOperations;
            _sessionHostsStartInfo = sessionHostsStartInfo;
        }

        public IDictionary<string, string> GetEnvironmentVariablesForSessionHost(int instanceNumber, string logFolderId, VmAgentSettings agentSettings)
        {
            // Note that most of these are being provided based on customer request
            var environmentVariables = new Dictionary<string, string>()
            {
                {
                    ConfigFileEnvVariable, GetGsdkConfigFilePath(_sessionHostsStartInfo.AssignmentId, instanceNumber)
                },
                {
                    ServerInstanceNumberEnvVariable, instanceNumber.ToString()
                },
                {
                    LogsDirectoryEnvVariable, GetLogFolder(logFolderId, VmConfiguration)
                },
                {
                    SharedContentFolderEnvVariable, GetSharedContentFolderPath()
                },
                {
                    CertificateFolderEnvVariable, GetCertificatesPath(_sessionHostsStartInfo.AssignmentId)
                },
                {
                    DumpsDirectoryEnvVariable, GetDumpFolder(logFolderId, VmConfiguration)
                }
            };

            environmentVariables.AddRange(VmConfiguration.GetCommonEnvironmentVariables(_sessionHostsStartInfo, VmConfiguration));

            return environmentVariables;
        }

        public void Create(int instanceNumber, string sessionHostUniqueId, string agentEndpoint, VmConfiguration vmConfiguration, string logFolderId)
        {
            Dictionary<string, string> certThumbprints =
                _sessionHostsStartInfo.GameCertificates?.Where(x => x.Thumbprint != null).ToDictionary(x => x.Name, x => x.Thumbprint);
            IDictionary<string, string> portMappings = GetPortMappingsDict(instanceNumber);
            if (_sessionHostsStartInfo.IsLegacy)
            {
                CreateLegacyGSDKConfigFile(instanceNumber, sessionHostUniqueId, certThumbprints, portMappings);
            }

            // If the title is marked as legacy GSDK, we support a smooth transition when they decide to use new GSDK
            // A title can start using new GSDK even if it's a legacy title
            CreateNewGSDKConfigFile(instanceNumber, sessionHostUniqueId, certThumbprints, portMappings, agentEndpoint, vmConfiguration, logFolderId);
        }

        private void CreateNewGSDKConfigFile(int instanceNumber, string sessionHostUniqueId, Dictionary<string, string> certThumbprints, IDictionary<string, string> portMappings, string agentEndpoint, VmConfiguration vmConfiguration, string logFolderId)
        {
            string configFilePath = Path.Combine(VmConfiguration.GetConfigRootFolderForSessionHost(instanceNumber),
                VmDirectories.GsdkConfigFilename);
            _logger.LogInformation($"Creating the configuration file at {configFilePath}");
            GsdkConfiguration gsdkConfig = new GsdkConfiguration
            {
                HeartbeatEndpoint = $"{agentEndpoint}:{VmConfiguration.ListeningPort}",
                SessionHostId = sessionHostUniqueId,
                VmId = vmConfiguration.VmId,
                LogFolder = GetLogFolder(logFolderId, vmConfiguration),
                CertificateFolder = vmConfiguration.VmDirectories.CertificateRootFolderContainer,
                SharedContentFolder = vmConfiguration.VmDirectories.GameSharedContentFolderContainer,
                GameCertificates = certThumbprints,
                BuildMetadata = _sessionHostsStartInfo.DeploymentMetadata,
                GamePorts = portMappings,
                PublicIpV4Address = _sessionHostsStartInfo.PublicIpV4Address,
                FullyQualifiedDomainName = _sessionHostsStartInfo.FQDN,
                ServerInstanceNumber = instanceNumber,
                GameServerConnectionInfo = GetGameServerConnectionInfo(instanceNumber)
            };

            string outputJson = JsonConvert.SerializeObject(gsdkConfig, Formatting.Indented, CommonSettings.JsonSerializerSettings);

            // This will overwrite the file if it was already there (which would happen when restarting containers for the same assignment)
            _systemOperations.FileWriteAllText(configFilePath, outputJson);
        }

        protected abstract string GetLogFolder(string logFolderId, VmConfiguration vmConfiguration);

        protected string GetDumpFolder(string logFolderId, VmConfiguration vmConfiguration)
        {
            return Path.Combine(GetLogFolder(logFolderId, vmConfiguration), VmDirectories.GameDumpsFolderName);
        }

        protected abstract string GetSharedContentFolder(VmConfiguration vmConfiguration);

        protected abstract string GetCertificateFolder(VmConfiguration vmConfiguration);

        private void CreateLegacyGSDKConfigFile(int instanceNumber, string sessionHostUniqueId, Dictionary<string, string> certThumbprints, IDictionary<string, string> portMappings)
        {
            // Legacy games are currently assumed to have only 1 asset.zip file which will have the game.exe as well
            // as the assets. We just place ServiceDefinition.json in that folder itself (since it contains game.exe).
            // This assumption will change later on and the code below will need to adapt.
            string configFilePath =
                Path.Combine(VmConfiguration.GetAssetExtractionFolderPathForSessionHost(instanceNumber, 0),
                    "ServiceDefinition.json");
            ServiceDefinition serviceDefinition;
            if (_systemOperations.FileExists(configFilePath))
            {
                _logger.LogInformation($"Parsing the existing service definition file at {configFilePath}.");
                serviceDefinition = JsonConvert.DeserializeObject<ServiceDefinition>(File.ReadAllText(configFilePath));
                serviceDefinition.JsonWorkerRole = serviceDefinition.JsonWorkerRole ?? new JsonWorkerRole();
            }
            else
            {
                _logger.LogInformation($"Creating the service definition file at {configFilePath}.");
                serviceDefinition = new ServiceDefinition
                {
                    JsonWorkerRole = new JsonWorkerRole()
                };
            }

            SetUpLegacyConfigValues(serviceDefinition, sessionHostUniqueId, instanceNumber, GetVmAgentIpAddress());

            certThumbprints?.ForEach(x => serviceDefinition.JsonWorkerRole.SetConfigValue(x.Key, x.Value));
            portMappings?.ForEach(x => serviceDefinition.JsonWorkerRole.SetConfigValue(x.Key, x.Value));
            _sessionHostsStartInfo.DeploymentMetadata?.ForEach(x => serviceDefinition.JsonWorkerRole.SetConfigValue(x.Key, x.Value));
            string outputJson = JsonConvert.SerializeObject(serviceDefinition, Formatting.Indented, CommonSettings.JsonSerializerSettings);

            // This will overwrite the file if it was already there (which would happen when restarting containers for the same assignment)
            _systemOperations.FileWriteAllText(configFilePath, outputJson);
        }

        private void SetUpLegacyConfigValues(ServiceDefinition serviceDefinition, string sessionHostId, int instanceNumber, string vmAgentHeartbeatIpAddress)
        {
            VmConfiguration.ParseAssignmentId(_sessionHostsStartInfo.AssignmentId, out Guid titleId, out Guid deploymentId, out string region);
            JsonWorkerRole workerRole = serviceDefinition.JsonWorkerRole;

            workerRole.NoGSMS = false;

            // Set the RoleInstanceId to vmId. Games like Activision's Call of Duty depend on this being unique per Vm in a cluster.
            // In v3, the vmId is globally unique (and should satisfy the requirement).
            serviceDefinition.CurrentRoleInstance = serviceDefinition.CurrentRoleInstance ?? new CurrentRoleInstance();
            serviceDefinition.CurrentRoleInstance.Id = VmConfiguration.VmId;

            // Not completely necessary, but avoids all VMs reporting the same deploymentId to the game server (and consequently to their lobby service potentially).
            serviceDefinition.DeploymentId = Guid.NewGuid().ToString("N");

            UpdateRoleInstanceEndpoints(serviceDefinition, instanceNumber);
            if (LegacyTitleHelper.LegacyTitleMappings.TryGetValue(titleId, out LegacyTitleDetails titleDetails))
            {
                workerRole.TitleId = titleDetails.TitleId.ToString();
                workerRole.GsiId = titleDetails.GsiId.ToString();
                workerRole.GsiSetId = titleDetails.GsiSetId.ToString();
                workerRole.ClusterId = GetHostNameFromFqdn(_sessionHostsStartInfo.FQDN);
            }
            else
            {
                workerRole.TitleId = VmConfiguration.GetPlayFabTitleId(titleId);
                workerRole.GsiId = deploymentId.ToString();
                workerRole.GsiSetId = deploymentId.ToString();

                // Some legacy games, such as SunsetOverdrive append "cloudapp.net" to the clusterId value
                // and try pinging that over the internet (essentially the server is pinging itself over the internet for 
                // health monitoring). Given that the FQDN in PlayFab doesn't necessarily end with cloudapp.net, we fake
                // this by specifying a dummy value here and editing the etc\hosts file to point dummyValue.cloudapp.net to
                // the publicIPAddress of the VM (available via environment variable).
                workerRole.ClusterId = "dummyValue";
            }

            workerRole.GsmsBaseUrl =
                $"http://{vmAgentHeartbeatIpAddress}:{VmConfiguration.ListeningPort}/v1";
            workerRole.SessionHostId =
                "r601y87miefd5ok8rdlgn-3b5np6glmoj4r24ymsk7gh7yhl-2019999738-wus.GSDKAgent_IN_0.Tenant_0.0";
            workerRole.InstanceId = sessionHostId;
            workerRole.ExeFolderPath = DefaultExePath;
            workerRole.TenantCount = _sessionHostsStartInfo.Count;
            workerRole.Location = LegacyAzureRegionHelper.GetRegionString(region);
            workerRole.Datacenter = workerRole.Location.Replace(" ", string.Empty);

            workerRole.XassBaseUrl = "https://service.auth.xboxlive.com/service/authenticate";
            workerRole.XastBaseUrl = "https://title.auth.xboxlive.com:10443/title/authenticate";
            workerRole.XstsBaseUrl = "https://xsts.auth.xboxlive.com/xsts/authorize";

            // Legacy Agent wrote it in this format.
            workerRole.TenantName = $"{instanceNumber,4:0000}";
            CertificateDetail ipSecCertificate = _sessionHostsStartInfo.IpSecCertificate ?? _sessionHostsStartInfo.XblcCertificate;
            if (!string.IsNullOrEmpty(_sessionHostsStartInfo.XblcCertificate?.Thumbprint))
            {
                // Halo GSDK needs both of these to start up properly.
                workerRole.XblGameServerCertificateThumbprint = _sessionHostsStartInfo.XblcCertificate?.Thumbprint;
                workerRole.XblIpsecCertificateThumbprint = ipSecCertificate?.Thumbprint;
            }

            // Add port mappings in the legacy format.
            GetLegacyPortMapping(instanceNumber).ToList().ForEach(port => workerRole.SetConfigValue(port.Key, port.Value));
        }

        private void UpdateRoleInstanceEndpoints(ServiceDefinition serviceDefinition, int instanceNumber)
        {
            // Sample:
            //"currentRoleInstance": {
            //    "id": "GSDKAgent_IN_0",
            //    "roleInstanceEndpoints": [
            //    {
            //        "name": "gametraffic",
            //        "internalPort": 7777,
            //        "externalPort": 31000,
            //        "maxPort": 30099,
            //        "minPort": 30000,
            //        "ipEndPoint": "10.26.114.84:7777",
            //        "publicIpEndPoint": "255.255.255.255:31000",
            //        "protocol": "udp"
            //    },
            //    {
            //        "name": "Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp",
            //        "internalPort": 3389,
            //        "externalPort": 3389,
            //        "maxPort": 30099,
            //        "minPort": 30000,
            //        "ipEndPoint": "10.26.114.84:3389",
            //        "publicIpEndPoint": "10.26.114.84:3389",
            //        "protocol": "tcp"
            //    },
            //    {
            //        "name": "Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput",
            //        "internalPort": 20000,
            //        "externalPort": 3389,
            //        "maxPort": 30099,
            //        "minPort": 30000,
            //        "ipEndPoint": "10.26.114.84:20000",
            //        "publicIpEndPoint": "255.255.255.255:3389",
            //        "protocol": "tcp"
            //    },
            //    {
            //        "name": "ShouldertapUdp",
            //        "internalPort": 10101,
            //        "externalPort": 30100,
            //        "maxPort": 30099,
            //        "minPort": 30000,
            //        "ipEndPoint": "10.26.114.84:10101",
            //        "publicIpEndPoint": "255.255.255.255:30100",
            //        "protocol": "udp"
            //    },
            //    {
            //        "name": "TCPEcho",
            //        "internalPort": 10100,
            //        "externalPort": 30000,
            //        "maxPort": 30099,
            //        "minPort": 30000,
            //        "ipEndPoint": "10.26.114.84:10100",
            //        "publicIpEndPoint": "255.255.255.255:30000",
            //        "protocol": "tcp"
            //    }
            //    ]
            //},
            serviceDefinition.CurrentRoleInstance.RoleInstanceEndpoints = new List<RoleInstanceEndpoint>();
            IList<PortMapping> portMappings = GetPortMappings(instanceNumber);
            foreach (PortMapping mapping in portMappings)
            {
                string internalServerListeningPort = GetLegacyServerListeningPort(mapping).ToString();
                serviceDefinition.CurrentRoleInstance.RoleInstanceEndpoints.Add(new RoleInstanceEndpoint()
                {
                    // The local ip can be a dummy value.
                    IpEndPoint = $"100.76.124.25:{internalServerListeningPort}",
                    Name = mapping.GamePort.Name,
                    Protocol = mapping.GamePort.Protocol.ToLower(),
                    PublicIpEndPoint = $"{_sessionHostsStartInfo.PublicIpV4Address}:{mapping.PublicPort}",
                    InternalPort = internalServerListeningPort,
                    ExternalPort = mapping.PublicPort.ToString()
                });
            }

            string hostnameFilePath =
                Path.Combine(VmConfiguration.GetAssetExtractionFolderPathForSessionHost(instanceNumber, 0),
                    "hostname");
            _systemOperations.FileWriteAllText(hostnameFilePath, _sessionHostsStartInfo.FQDN);

            if (_sessionHostsStartInfo.IpSecCertificate != null)
            {
                string ipsecFileNamePath =
                    Path.Combine(VmConfiguration.GetAssetExtractionFolderPathForSessionHost(instanceNumber, 0),
                        "ipsec_certificate_thumbprint");
                _systemOperations.FileWriteAllText(ipsecFileNamePath, _sessionHostsStartInfo.IpSecCertificate.Thumbprint);
            }
        }

        private string GetHostNameFromFqdn(string fqdn)
        {
            return fqdn.Split('.', StringSplitOptions.RemoveEmptyEntries)[0];
        }

        /// <summary>
        /// Many of the legacy game servers require port mappings to be written in the config file in a specific format.
        /// For example, if the build specifies a port as follows:
        /// Name: GameUDP, port : 8080, Protocol: UDP
        /// The legacy game servers would need two config values for this port:
        /// "PortGameUDPInternal" - this is the port that game server should listen on within the VM.
        /// "PortGameUDPExternal" - this is the port that clients can reach the game server on.
        /// 
        /// Essentially, the name is of the following format: "Port{name_in_build_configuration_}Internal" and "Port{name_in_build_configuration_}External".
        /// </summary>
        /// <param name="instanceNumber">The game server instance number (between 1 and number of servers per Vm).</param>
        /// <returns></returns>
        protected abstract Dictionary<string, string> GetLegacyPortMapping(int instanceNumber);

        /// <summary>
        /// Gets the port at which the game server should listen on (differs between containers and processes).
        /// </summary>
        /// <param name="portMapping"></param>
        /// <returns></returns>
        protected abstract int GetLegacyServerListeningPort(PortMapping portMapping);

        public abstract IList<PortMapping> GetPortMappings(int instanceNumber);

        /// <summary>
        /// Gets the game server connection information (IP Address and ports of the server).
        /// </summary>
        /// <param name="instanceNumber">The instance of game server running on the VM.</param>
        /// <returns></returns>
        private GameServerConnectionInfo GetGameServerConnectionInfo(int instanceNumber)
        {
            return new GameServerConnectionInfo()
            {
                PublicIpV4Adress = _sessionHostsStartInfo.PublicIpV4Address,
                GamePortsConfiguration = GetGamePortConfiguration(instanceNumber)
            };
        }

        /// <summary>
        /// Gets the ports assigned to the specific instance of the game server.
        /// The ports include both, the port at which the game server listens on,
        /// and the port to which the clients connect to (which internally maps to the server listening port). 
        /// </summary>
        /// <param name="instanceNumber">The instance number of the game server on the VM</param>
        private IEnumerable<GamePort> GetGamePortConfiguration(int instanceNumber)
        {
            IList<PortMapping> portMappings = GetPortMappings(instanceNumber);
            return portMappings.Select(port => new GamePort()
            {
                Name = port.GamePort.Name,
                ServerListeningPort = port.GamePort.Number,
                ClientConnectionPort = port.PublicPort
            });
        }

        private IDictionary<string, string> GetPortMappingsDict(int instanceNumber)
        {
            return GetPortMappings(instanceNumber)?.ToDictionary(x => x.GamePort.Name, x => x.GamePort.Number.ToString());
        }

        private string GetVmAgentIpAddress()
        {
            return GetVmAgentIpAddressInternal();
        }

        protected abstract string GetVmAgentIpAddressInternal();
    }
}
