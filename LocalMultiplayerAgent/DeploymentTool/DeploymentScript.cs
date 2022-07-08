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
{//hello
    public class DeploymentScript
    {
        private readonly MultiplayerSettings settings;
        private readonly DeploymentSettings settingsDeployment;
        public DeploymentScript(MultiplayerSettings multiplayerSettings)
        {
            settings = multiplayerSettings;
            settingsDeployment = JsonConvert.DeserializeObject<DeploymentSettings>(File.ReadAllText("DeploymentTool/deployment.json"));
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

            if (res.Error != null)
            {
                Console.WriteLine(res.Error.ErrorMessage);
                return;
            }

            // TODO: do validation checks

            dynamic createBuild = null;
            if (settings.RunContainer)
            {
                if (settingsDeployment.OSPlatform == "Windows")
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
                    //for CustomLinux Container
                    //yet to be implemented
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
                BuildName = settingsDeployment.BuildName,
                VmSize = (AzureVmSize)Enum.Parse(typeof(AzureVmSize), settingsDeployment.VmSize),
                ContainerFlavor = ContainerFlavor.CustomLinux,
                ContainerImageReference = new ContainerImageReference()
                {
                    ImageName = settings.ContainerStartParameters.ImageDetails.ImageName,
                    Tag = settings.ContainerStartParameters.ImageDetails.ImageTag
                },
                Ports = PortMapping(),
                ContainerRunCommand = settings.ContainerStartParameters.StartGameCommand,
                RegionConfigurations = settingsDeployment.RegionConfigurations?.Select(x => new BuildRegionParams()
                {
                    Region = x.Region,
                    MaxServers = x.MaxServers,
                    StandbyServers = x.StandbyServers,
                    MultiplayerServerCountPerVm = settingsDeployment.MultiplayerServerCountPerVm,
                    VmSize = (AzureVmSize)Enum.Parse(typeof(AzureVmSize), settingsDeployment.VmSize)

                }).ToList(),
                MultiplayerServerCountPerVm = settingsDeployment.MultiplayerServerCountPerVm,
            };
        }

        public CreateBuildWithManagedContainerRequest GetManagedContainerRequest()
        {
            return new CreateBuildWithManagedContainerRequest
            {
                VmSize = (AzureVmSize)Enum.Parse(typeof(AzureVmSize), settingsDeployment.VmSize),
                GameCertificateReferences = null,
                Ports = PortMapping(),
                MultiplayerServerCountPerVm = settingsDeployment.MultiplayerServerCountPerVm,
                RegionConfigurations = settingsDeployment.RegionConfigurations?.Select(x => new BuildRegionParams()
                {
                    Region = x.Region,
                    MaxServers = x.MaxServers,
                    StandbyServers = x.StandbyServers,
                    MultiplayerServerCountPerVm = settingsDeployment.MultiplayerServerCountPerVm,
                    VmSize = (AzureVmSize)Enum.Parse(typeof(AzureVmSize), settingsDeployment.VmSize)

                }).ToList(),
                BuildName = settingsDeployment.BuildName,
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
                VmSize = (AzureVmSize)Enum.Parse(typeof(AzureVmSize), settingsDeployment.VmSize),
                GameCertificateReferences = null,
                Ports = PortMapping(),
                MultiplayerServerCountPerVm = settingsDeployment.MultiplayerServerCountPerVm,
                RegionConfigurations = settingsDeployment.RegionConfigurations?.Select(x => new BuildRegionParams()
                {
                    Region = x.Region,
                    MaxServers = x.MaxServers,
                    StandbyServers = x.StandbyServers,
                    MultiplayerServerCountPerVm = settingsDeployment.MultiplayerServerCountPerVm,
                    VmSize = (AzureVmSize)Enum.Parse(typeof(AzureVmSize), settingsDeployment.VmSize)

                }).ToList(),
                BuildName = settingsDeployment.BuildName,
                GameAssetReferences = settings.AssetDetails?.Select(x => new AssetReferenceParams()
                {
                    FileName = GetAssetFileNameFromPath(x.LocalFilePath),
                }).ToList(),
                StartMultiplayerServerCommand = settings.ProcessStartParameters.StartGameCommand,
                OsPlatform = settingsDeployment.OSPlatform
            };
        }

        public async Task<PlayFabResult<CreateBuildWithProcessBasedServerResponse>> CreateBuildWithProcessBasedServer(CreateBuildWithProcessBasedServerRequest request)
        {
            Console.WriteLine($"Starting deployment {request.BuildName} for titleId, regions  {string.Join(", ", request.RegionConfigurations.Select(x => x.Region))}");

            return await PlayFabMultiplayerAPI.CreateBuildWithProcessBasedServerAsync(request);
        }

        public async Task<PlayFabResult<CreateBuildWithManagedContainerResponse>> CreateBuildWithManagedContainer(CreateBuildWithManagedContainerRequest request)
        {
            Console.WriteLine($"Starting deployment {request.BuildName} for titleId, regions  {string.Join(", ", request.RegionConfigurations.Select(x => x.Region))}");

            return await PlayFabMultiplayerAPI.CreateBuildWithManagedContainerAsync(request);
        }

        public async Task<PlayFabResult<CreateBuildWithCustomContainerResponse>> CreateBuildWithCustomContainer(CreateBuildWithCustomContainerRequest request)
        {
            Console.WriteLine($"Starting deployment {request.BuildName} for titleId, regions  {string.Join(", ", request.RegionConfigurations.Select(x => x.Region))}");

            return await PlayFabMultiplayerAPI.CreateBuildWithCustomContainerAsync(request);
        }

        public List<PlayFab.MultiplayerModels.Port> PortMapping()
        {
            List<PlayFab.MultiplayerModels.Port> ports = null;

            foreach (var portList in settings.PortMappingsList)
            {
                ports = portList?.Select(x => new PlayFab.MultiplayerModels.Port()
                {
                    Name = x.GamePort.Name,
                    Num = settings.RunContainer ? x.GamePort.Number : 0,
                    Protocol = (ProtocolType)Enum.Parse(typeof(ProtocolType), x.GamePort.Protocol)
                }).ToList();
            }

            return ports;
        }

        public string GetAssetFileNameFromPath(string filePath)
        {
            return System.IO.Path.GetFileName(filePath);
        }

        public async Task<PlayFabResult<GetAssetDownloadUrlResponse>> FileExistsInBlob(string filename)
        {
            GetAssetDownloadUrlRequest downloadRequest = new() { FileName = filename };

            return await PlayFabMultiplayerAPI.GetAssetDownloadUrlAsync(downloadRequest);
        }

        public async Task CheckAssetFiles(string filename)
        {
           
            var filevalidator = FileExistsInBlob(filename);

            if (filevalidator.Result.Result == null)
            {
                GetAssetUploadUrlRequest request1 = new() { FileName = filename };

                //TODO: log progress of asset upload
                var uriResult = await PlayFabMultiplayerAPI.GetAssetUploadUrlAsync(request1);

                if (uriResult.Error != null)
                {
                    Console.WriteLine(uriResult.Error.ErrorMessage);
                }
                var uri = new System.Uri(uriResult.Result.AssetUploadUrl);

                var blockBlob = new CloudBlockBlob(uri);
                await blockBlob.UploadFromFileAsync(filename);
            }

            else if (filevalidator.Result.Error != null)
            {
                Console.WriteLine($"{filevalidator.Result.Error.ErrorMessage}");
            }
        }
    }
 
}

  
