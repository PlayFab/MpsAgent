using Microsoft.Azure.Gaming.LocalMultiplayerAgent.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.MultiplayerModels;
using System.IO;
using Microsoft.Azure.Gaming.LocalMultiplayerAgent.DeploymentTool;
using Microsoft.Azure.Storage.Blob;

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.MPSDeploymentTool
{
    public class DeploymentScript
    {
        private readonly MultiplayerSettings settings;
        private readonly DeploymentSettings deploymentSettings;
        public DeploymentScript(MultiplayerSettings multiplayerSettings)
        {
            settings = multiplayerSettings ?? throw new ArgumentNullException(nameof(multiplayerSettings));
            deploymentSettings = JsonConvert.DeserializeObject<DeploymentSettings>(File.ReadAllText("DeploymentTool/deployment.json"));
        }

        public async Task RunScriptAsync()
        { 
            PlayFabSettings.staticSettings.TitleId = settings.TitleId;

            string secret = Environment.GetEnvironmentVariable("PF_SECRET");
            if (string.IsNullOrEmpty(secret))
            {
                Console.WriteLine("Enter developer secret key");
                PlayFabSettings.staticSettings.DeveloperSecretKey = Console.ReadLine();
            }

            var req = new PlayFab.AuthenticationModels.GetEntityTokenRequest();
            var res = await PlayFabAuthenticationAPI.GetEntityTokenAsync(req);

            PrintError(res.Error);

            // TODO: do validation checks
            DeploymentSettingsValidator validator = new DeploymentSettingsValidator(deploymentSettings);

            if (!validator.IsValid())
            {
                Console.WriteLine("The specified settings are invalid. Please correct them and re-run the agent.");
                Environment.Exit(1);
            }

            dynamic createBuild = null;
            if (settings.RunContainer)
            {
                if (deploymentSettings.OSPlatform == "Windows")
                {
                    CreateBuildWithManagedContainerRequest request = GetManagedContainerRequest();

                    foreach (var file in request.GameAssetReferences)
                    {
                        await CheckAssetFiles(file.FileName);
                    }

                    createBuild = await CreateBuildWithManagedContainer(request);
                }
                else
                {
                    //TODO: for CustomLinux Container
                }
            }
            else
            {
                CreateBuildWithProcessBasedServerRequest request = GetProcessBasedServerRequest();

                foreach (var file in request.GameAssetReferences)
                {
                    await CheckAssetFiles(file.FileName);
                }

                createBuild = await CreateBuildWithProcessBasedServer(request);
            }

            if (createBuild.Error != null)
            {
                foreach (var error in createBuild.Error.ErrorDetails)
                {
                    foreach (var errorMessage in error.Value)
                    {
                        Console.WriteLine($"{errorMessage}");
                    }
                }
            }
        }

        public CreateBuildWithCustomContainerRequest GetCustomContainerRequest()
        {
            return new CreateBuildWithCustomContainerRequest
            {
                BuildName = deploymentSettings.BuildName,
                VmSize = Enum.Parse<AzureVmSize>(deploymentSettings.VmSize),
                ContainerFlavor = ContainerFlavor.CustomLinux,
                ContainerImageReference = new ContainerImageReference()
                {
                    ImageName = settings.ContainerStartParameters.ImageDetails.ImageName,
                    Tag = settings.ContainerStartParameters.ImageDetails.ImageTag
                },
                Ports = PortMapping(),
                ContainerRunCommand = settings.ContainerStartParameters.StartGameCommand,
                RegionConfigurations = deploymentSettings.RegionConfigurations?.Select(x => new BuildRegionParams()
                {
                    Region = x.Region,
                    MaxServers = x.MaxServers,
                    StandbyServers = x.StandbyServers,
                    MultiplayerServerCountPerVm = deploymentSettings.MultiplayerServerCountPerVm,
                    VmSize = Enum.Parse<AzureVmSize>(deploymentSettings.VmSize)

                }).ToList(),
                MultiplayerServerCountPerVm = deploymentSettings.MultiplayerServerCountPerVm,
            };
        }

        public CreateBuildWithManagedContainerRequest GetManagedContainerRequest()
        {
            return new CreateBuildWithManagedContainerRequest
            {
                VmSize = Enum.Parse<AzureVmSize>(deploymentSettings.VmSize),
                GameCertificateReferences = null,
                Ports = PortMapping(),
                MultiplayerServerCountPerVm = deploymentSettings.MultiplayerServerCountPerVm,
                RegionConfigurations = deploymentSettings.RegionConfigurations?.Select(x => new BuildRegionParams()
                {
                    Region = x.Region,
                    MaxServers = x.MaxServers,
                    StandbyServers = x.StandbyServers,
                    MultiplayerServerCountPerVm = deploymentSettings.MultiplayerServerCountPerVm,
                    VmSize = Enum.Parse<AzureVmSize>(deploymentSettings.VmSize)

                }).ToList(),
                BuildName = deploymentSettings.BuildName,
                GameAssetReferences = settings.AssetDetails?.Select(x => new AssetReferenceParams()
                {
                    FileName = GetAssetFileNameFromPath(x.LocalFilePath),
                    MountPath = x.MountPath
                }).ToList(),
                StartMultiplayerServerCommand = settings.ContainerStartParameters.StartGameCommand,
                ContainerFlavor = ContainerFlavor.ManagedWindowsServerCore
            };
        }

        

        public CreateBuildWithProcessBasedServerRequest GetProcessBasedServerRequest()
        {
            return new CreateBuildWithProcessBasedServerRequest
            {
                VmSize = Enum.Parse<AzureVmSize>(deploymentSettings.VmSize),
                GameCertificateReferences = null,
                Ports = PortMapping(),
                MultiplayerServerCountPerVm = deploymentSettings.MultiplayerServerCountPerVm,
                RegionConfigurations = deploymentSettings.RegionConfigurations?.Select(x => new BuildRegionParams()
                {
                    Region = x.Region,
                    MaxServers = x.MaxServers,
                    StandbyServers = x.StandbyServers,
                    MultiplayerServerCountPerVm = deploymentSettings.MultiplayerServerCountPerVm,
                    VmSize = Enum.Parse<AzureVmSize>(deploymentSettings.VmSize)

                }).ToList(),
                BuildName = deploymentSettings.BuildName,
                GameAssetReferences = settings.AssetDetails?.Select(x => new AssetReferenceParams()
                {
                    FileName = GetAssetFileNameFromPath(x.LocalFilePath),
                }).ToList(),
                StartMultiplayerServerCommand = settings.ProcessStartParameters.StartGameCommand,
                OsPlatform = deploymentSettings.OSPlatform
            };
        }

        public void PrintDeploymentMessage(string buildName, List<BuildRegionParams> regionConfigurations)
        {
            Console.WriteLine($"Starting deployment {buildName} for titleId, regions  {string.Join(", ", regionConfigurations.Select(x => x.Region))}");
        }

        public async Task<PlayFabResult<CreateBuildWithProcessBasedServerResponse>> CreateBuildWithProcessBasedServer(CreateBuildWithProcessBasedServerRequest request)
        {
            PrintDeploymentMessage(request.BuildName, request.RegionConfigurations);

            return await PlayFabMultiplayerAPI.CreateBuildWithProcessBasedServerAsync(request);
        }

        public async Task<PlayFabResult<CreateBuildWithManagedContainerResponse>> CreateBuildWithManagedContainer(CreateBuildWithManagedContainerRequest request)
        {
            PrintDeploymentMessage(request.BuildName, request.RegionConfigurations);

            return await PlayFabMultiplayerAPI.CreateBuildWithManagedContainerAsync(request);
        }

        public async Task<PlayFabResult<CreateBuildWithCustomContainerResponse>> CreateBuildWithCustomContainer(CreateBuildWithCustomContainerRequest request)
        {
            PrintDeploymentMessage(request.BuildName, request.RegionConfigurations);

            return await PlayFabMultiplayerAPI.CreateBuildWithCustomContainerAsync(request);
        }

        public List<PlayFab.MultiplayerModels.Port> PortMapping()
        {
            var ports = new List<Port>();

            foreach (var portList in settings.PortMappingsList)
            {
                ports.AddRange(portList?.Select(x => new Port()
                {
                    Name = x.GamePort.Name,
                    Num = settings.RunContainer ? x.GamePort.Number : 0,
                    Protocol = (ProtocolType)Enum.Parse(typeof(ProtocolType), x.GamePort.Protocol)
                }).ToList());
            }

            return ports;
        }

        public string GetAssetFileNameFromPath(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public async Task<PlayFabResult<GetAssetDownloadUrlResponse>> FileExistsInBlob(string filename)
        {
            GetAssetDownloadUrlRequest downloadRequest = new GetAssetDownloadUrlRequest() { FileName = filename };

            return await PlayFabMultiplayerAPI.GetAssetDownloadUrlAsync(downloadRequest);
        }

        public void PrintError(dynamic error)
        {
            if (error != null && error.ErrorMessage != null)
            {
                Console.WriteLine($"{error.ErrorMessage}");
            }
        }

        public async Task CheckAssetFiles(string filename)
        {
           
            var filevalidator = FileExistsInBlob(filename);

            if (filevalidator.Result.Result == null)
            {
                GetAssetUploadUrlRequest request1 = new GetAssetUploadUrlRequest() { FileName = filename };

                //TODO: log progress of asset upload
                var uriResult = await PlayFabMultiplayerAPI.GetAssetUploadUrlAsync(request1);

                PrintError(uriResult.Error);

                var uri = new System.Uri(uriResult.Result.AssetUploadUrl);

                var blockBlob = new CloudBlockBlob(uri);
                await blockBlob.UploadFromFileAsync(filename);
            }

            else
            {
                PrintError(filevalidator.Result.Error);
                return;
            }
        }
    }
 
}

  
