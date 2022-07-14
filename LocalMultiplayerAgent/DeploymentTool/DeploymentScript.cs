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
                Console.WriteLine("Deploying to PlayFab Multiplayer Servers...\nEnter developer secret key");
                PlayFabSettings.staticSettings.DeveloperSecretKey = Console.ReadLine();
            }

            var tokenReq = new PlayFab.AuthenticationModels.GetEntityTokenRequest();
            var tokenRes = await PlayFabAuthenticationAPI.GetEntityTokenAsync(tokenReq);

            if (tokenRes.Error != null && tokenRes.Error.ErrorMessage != null)
            {
                Console.WriteLine($"{tokenRes.Error.ErrorMessage}");
                Environment.Exit(1);
            }

            DeploymentSettingsValidator validator = new DeploymentSettingsValidator(deploymentSettings);

            if (!validator.IsValid())
            {
                Console.WriteLine("The specified settings are invalid. Please correct them and re-run the agent.");
                Environment.Exit(1);
            }

            dynamic createBuild;
            if (settings.RunContainer)
            {
                if (deploymentSettings.OSPlatform == "Windows")
                {
                    CreateBuildWithManagedContainerRequest request = GetManagedContainerRequest();

                    foreach (var file in request.GameAssetReferences)
                    {
                        await CheckAssetFilesAsync(file.FileName);
                    }

                    createBuild = await CreateBuildWithManagedContainerAsync(request);
                }
                else
                {
                    CreateBuildWithCustomContainerRequest request = GetCustomContainerRequest();

                    foreach (var file in request.GameAssetReferences)
                    {
                        await CheckAssetFilesAsync(file.FileName);
                    }

                    createBuild = await CreateBuildWithCustomContainerAsync(request);
                }
            }
            else
            {
                CreateBuildWithProcessBasedServerRequest request = GetProcessBasedServerRequest();

                foreach (var file in request.GameAssetReferences)
                {
                    await CheckAssetFilesAsync(file.FileName);
                }

                createBuild = await CreateBuildWithProcessBasedServerAsync(request);
            }

            if (createBuild.Error != null)
            {
                Console.WriteLine("Failed to successfully create build: \n");
                foreach (var error in createBuild.Error.ErrorDetails)
                {
                    foreach (var errorMessage in error.Value)
                    {
                        Console.WriteLine($"{errorMessage}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Build creation was successful!");
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
                GameAssetReferences = settings.AssetDetails != null ? settings.AssetDetails?.Select(x => new AssetReferenceParams()
                {
                    FileName = GetAssetFileNameFromPath(x.LocalFilePath),
                    MountPath = x.MountPath
                }).ToList() : new List<AssetReferenceParams>(),
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
                GameAssetReferences = settings.AssetDetails != null ? settings.AssetDetails?.Select(x => new AssetReferenceParams()
                {
                    FileName = GetAssetFileNameFromPath(x.LocalFilePath),
                    MountPath = x.MountPath
                }).ToList() : new List<AssetReferenceParams>(),
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
                GameAssetReferences = settings.AssetDetails != null ? settings.AssetDetails?.Select(x => new AssetReferenceParams()
                {
                    FileName = GetAssetFileNameFromPath(x.LocalFilePath),
                }).ToList() : new List<AssetReferenceParams>(),
                StartMultiplayerServerCommand = settings.ProcessStartParameters.StartGameCommand,
                OsPlatform = deploymentSettings.OSPlatform
            };
        }

        public void PrintDeploymentMessage(string buildName, List<BuildRegionParams> regionConfigurations)
        {
            Console.WriteLine($"Starting deployment {buildName} for titleId, regions  {string.Join(", ", regionConfigurations.Select(x => x.Region))}");
        }

        public async Task<PlayFabResult<CreateBuildWithProcessBasedServerResponse>> CreateBuildWithProcessBasedServerAsync(CreateBuildWithProcessBasedServerRequest request)
        {
            PrintDeploymentMessage(request.BuildName, request.RegionConfigurations);

            return await PlayFabMultiplayerAPI.CreateBuildWithProcessBasedServerAsync(request);
        }

        public async Task<PlayFabResult<CreateBuildWithManagedContainerResponse>> CreateBuildWithManagedContainerAsync(CreateBuildWithManagedContainerRequest request)
        {
            PrintDeploymentMessage(request.BuildName, request.RegionConfigurations);

            return await PlayFabMultiplayerAPI.CreateBuildWithManagedContainerAsync(request);
        }

        public async Task<PlayFabResult<CreateBuildWithCustomContainerResponse>> CreateBuildWithCustomContainerAsync(CreateBuildWithCustomContainerRequest request)
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

        public async Task CheckAssetFilesAsync(string filename)
        {
            GetAssetUploadUrlRequest request1 = new GetAssetUploadUrlRequest() { FileName = filename };
            
            var uriResult = await PlayFabMultiplayerAPI.GetAssetUploadUrlAsync(request1);

            if (uriResult.Error != null)
            {
                if (uriResult.Error.ErrorMessage.Contains("AssetAlreadyExists"))
                {
                    Console.WriteLine($"{filename} is already uploaded. Going ahead with deployment...");
                }
                else
                {
                    Console.WriteLine(uriResult.Error.ErrorMessage);
                    Environment.Exit(1);   
                }
            }
            else
            {
                try
                {
                    //TODO: log progress of asset upload
                    var uri = new Uri(uriResult.Result.AssetUploadUrl);
                    var blockBlob = new CloudBlockBlob(uri);

                    Console.WriteLine($"Uploading {filename}...");
                    await blockBlob.UploadFromFileAsync(filename);
                    Console.WriteLine($"Uploading {filename} successful!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Environment.Exit(1);
                }
            }
        }
    }
}