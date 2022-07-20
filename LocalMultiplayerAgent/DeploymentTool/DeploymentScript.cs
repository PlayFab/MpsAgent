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
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.MPSDeploymentTool
{
    public class DeploymentScript
    {
        private readonly MultiplayerSettings settings;
        private readonly DeploymentSettings deploymentSettings;

        public DeploymentScript(MultiplayerSettings multiplayerSettings)
        {
            settings = multiplayerSettings ?? throw new ArgumentNullException(nameof(multiplayerSettings));
            deploymentSettings = JsonConvert.DeserializeObject<DeploymentSettings>(File.ReadAllText("DeploymentTool/DeploymentSettings.json"));
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

            await CheckAndUploadCertificatesAsync();

            foreach (var file in settings.AssetDetails)
            {
                await CheckAndUploadAssetFileAsync(file.LocalFilePath);
            }

            dynamic createBuild;

            if (settings.RunContainer)
            {
                if (Globals.GameServerEnvironment == GameServerEnvironment.Windows)
                {
                    CreateBuildWithManagedContainerRequest request = GetManagedContainerRequest();
                    createBuild = await CreateBuildWithManagedContainerAsync(request);
                }
                else
                {
                    await CheckAndUploadLinuxContainerImageAsync();
                    CreateBuildWithCustomContainerRequest request = GetCustomContainerRequest();
                    createBuild = await CreateBuildWithCustomContainerAsync(request);
                }
            }
            else
            {
                CreateBuildWithProcessBasedServerRequest request = GetProcessBasedServerRequest();
                createBuild = await CreateBuildWithProcessBasedServerAsync(request);
            }

            if (createBuild.Error != null)
            {
                Console.WriteLine("Failed to successfully create build");
                if(createBuild.Error.ErrorMessage != null)
                {
                    Console.WriteLine($"{createBuild.Error.ErrorMessage}");
                }
                else if (createBuild.Error.ErrorDetails.Any())
                {
                    foreach (var error in createBuild.Error.ErrorDetails)
                    {
                        foreach (var errorMessage in error.Value)
                        {
                            Console.WriteLine($"{errorMessage}");
                        }
                    }
                    Console.WriteLine($"{createBuild.Error.ErrorMessage}");
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
                GameCertificateReferences = settings.GameCertificateDetails != null ? settings.GameCertificateDetails.Select(x => new GameCertificateReferenceParams()
                {
                    Name = x.Name
                }).ToList() : new List<GameCertificateReferenceParams>(),
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
                    FileName = x.LocalFilePath,
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
                GameCertificateReferences = settings.GameCertificateDetails != null ? settings.GameCertificateDetails.Select(x => new GameCertificateReferenceParams()
                {
                    Name = x.Name
                }).ToList() : new List<GameCertificateReferenceParams>(),
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
                GameAssetReferences = settings.AssetDetails != null ? settings.AssetDetails.Select(x => new AssetReferenceParams()
                {
                    FileName = x.LocalFilePath,
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
                GameCertificateReferences = settings.GameCertificateDetails != null ? settings.GameCertificateDetails.Select(x => new GameCertificateReferenceParams()
                {
                    Name = x.Name
                }).ToList() : new List<GameCertificateReferenceParams>(),
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
                GameAssetReferences = settings.AssetDetails != null ? settings.AssetDetails.Select(x => new AssetReferenceParams()
                {
                    FileName = x.LocalFilePath,
                }).ToList() : new List<AssetReferenceParams>(),
                StartMultiplayerServerCommand = settings.ProcessStartParameters.StartGameCommand,
                OsPlatform = Globals.GameServerEnvironment.ToString()
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

        public async Task CheckAndUploadLinuxContainerImageAsync(string imageSkipToken = null, int pageSize = 10)
        {
            while (string.IsNullOrEmpty(imageSkipToken))
            {
                var ContainerImagesRequest = new ListContainerImagesRequest
                {
                    SkipToken = imageSkipToken,
                    PageSize = pageSize
                };
                var ContainerImagesResponse = await PlayFabMultiplayerAPI.ListContainerImagesAsync(ContainerImagesRequest);

                if (ContainerImagesResponse.Error != null)
                {
                    Console.WriteLine(ContainerImagesResponse.Error.ErrorMessage);
                    Environment.Exit(1);
                }

                else
                {
                    imageSkipToken = ContainerImagesResponse.Result.SkipToken ?? null;
                    IEnumerable<string> existingImages = ContainerImagesResponse.Result.Images.Where(x => x == settings.ContainerStartParameters.ImageDetails.ImageName);
                    if (!existingImages.Any() && string.IsNullOrEmpty(imageSkipToken))
                    {
                        //TODO: upload LinuxContainer image API
                        //This is currently non-existent 
                        Console.WriteLine("Make sure you have uploaded your Linux container image to Docker before attempting deploy");
                    }
                }
            }
        }

        public async Task<PlayFabResult<EmptyResponse>> UploadCertificateAsync(GameCertificateDetails certificate)
        {
            X509Certificate2 certCopy = new X509Certificate2(certificate.Path);
            var uploadCertificateRequest = new UploadCertificateRequest
            {
                GameCertificate = new Certificate
                {
                    Name = certificate.Name,
                    Base64EncodedValue = Convert.ToBase64String(certCopy.RawData)
                    //Password = ""  //Only passwordless certificates supported currently
                }
            };

            Console.WriteLine($"Uploading {certificate.Name}...");

            return await PlayFabMultiplayerAPI.UploadCertificateAsync(uploadCertificateRequest);
        }

        public async Task CheckAndUploadCertificatesAsync(string certSkipToken = null, int pageSize = 10)
        {
            List<string> failedCertUploads = null;

            while (string.IsNullOrEmpty(certSkipToken))
            {
                var certificateSummariesRequest = new ListCertificateSummariesRequest
                {
                    SkipToken = certSkipToken,
                    PageSize = pageSize
                };
                var certificateSummariesResponse = await PlayFabMultiplayerAPI.ListCertificateSummariesAsync(certificateSummariesRequest);

                if (certificateSummariesResponse.Error != null)
                {
                    Console.WriteLine(certificateSummariesResponse.Error.ErrorMessage);
                    Environment.Exit(1);
                }

                else
                {
                    certSkipToken = certificateSummariesResponse.Result.SkipToken ?? null;

                    foreach (var certificate in settings.GameCertificateDetails)
                    {
                        IEnumerable<CertificateSummary> existingCerts = certificateSummariesResponse.Result.CertificateSummaries.Where(x => x.Name == certificate.Name);
                        if (!existingCerts.Any() && string.IsNullOrEmpty(certSkipToken))
                        {
                            var uploadCertificateRes = await UploadCertificateAsync(certificate);

                            if (uploadCertificateRes.Error != null)
                            {
                                Console.WriteLine(uploadCertificateRes.Error.ErrorMessage);
                                failedCertUploads.Add(certificate.Name);
                                continue;
                            }

                            else
                            {
                                Console.WriteLine($"Uploading {certificate.Name} successful!");
                            }
                        }
                    } 
                }
            }

            if (failedCertUploads?.Count > 0)
            {
                Console.WriteLine($"The folowing certificates failed in the upload process: {string.Join(", ", failedCertUploads)}");
            }
        }

        public List<Port> PortMapping()
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

        public async Task CheckAndUploadAssetFileAsync(string fileNamePath)
        {
            string fileName = Path.GetFileName(fileNamePath);
            GetAssetUploadUrlRequest request1 = new GetAssetUploadUrlRequest() { FileName = fileName };

            var uriResult = await PlayFabMultiplayerAPI.GetAssetUploadUrlAsync(request1);
            
            if (uriResult.Error != null)
            {
                if (uriResult.Error.ErrorMessage.Contains("AssetAlreadyExists"))
                {
                    Console.WriteLine($"{fileName} is already uploaded. Going ahead with deployment...");
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

                    Console.WriteLine($"Uploading {fileName}...");

                    await blockBlob.UploadFromFileAsync(fileNamePath);

                    Console.WriteLine($"Uploading {fileName} successful!");
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