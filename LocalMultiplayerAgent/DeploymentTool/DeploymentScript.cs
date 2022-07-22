using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Microsoft.Azure.Gaming.LocalMultiplayerAgent.DeploymentTool;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Gaming.LocalMultiplayerAgent.Config;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.MultiplayerModels;

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

            Console.WriteLine("Deploying to PlayFab Multiplayer Servers...\n");

            var auth = await PlayFabAuthentication();

            ErrorCheck(auth.Error);

            DeploymentSettingsValidator validator = new DeploymentSettingsValidator(deploymentSettings);

            if (!validator.IsValid())
            {
                Console.WriteLine("The specified settings are invalid. Please correct them and re-run the agent.");
                Environment.Exit(1);
            }

            await CheckAndUploadCertificatesAsync();

            if (Globals.GameServerEnvironment == GameServerEnvironment.Windows)
            {
                foreach (var file in settings.AssetDetails)
                {
                    await CheckAndUploadAssetFileAsync(file.LocalFilePath);
                }
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
                //call BuildSummariesAPI to confirm complete deployment ??
                Console.WriteLine("Build creation was successful!");
            }
        }

        public async Task<PlayFabResult<PlayFab.AuthenticationModels.GetEntityTokenResponse>> PlayFabAuthentication()
        {
            string secret = Environment.GetEnvironmentVariable("PF_SECRET", EnvironmentVariableTarget.Process);
            PlayFabSettings.staticSettings.DeveloperSecretKey = secret ?? null;
            if (string.IsNullOrEmpty(secret))
            {
                Console.WriteLine("Enter developer secret key");
                PlayFabSettings.staticSettings.DeveloperSecretKey = Console.ReadLine();
            }

            var tokenReq = new PlayFab.AuthenticationModels.GetEntityTokenRequest();
            return await PlayFabAuthenticationAPI.GetEntityTokenAsync(tokenReq);
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
                    FileName = Path.GetFileName(x.LocalFilePath),
                    MountPath = x.MountPath
                }).ToList() : new List<AssetReferenceParams>(),
                MultiplayerServerCountPerVm = deploymentSettings.MultiplayerServerCountPerVm,
                Metadata = (Dictionary<string, string>)settings.DeploymentMetadata ?? new Dictionary<string, string>()
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
                    FileName = Path.GetFileName(x.LocalFilePath),
                    MountPath = x.MountPath
                }).ToList() : new List<AssetReferenceParams>(),
                StartMultiplayerServerCommand = settings.ContainerStartParameters.StartGameCommand,
                ContainerFlavor = ContainerFlavor.ManagedWindowsServerCore,
                Metadata = (Dictionary<string, string>)settings.DeploymentMetadata ?? new Dictionary<string, string>()
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
                    FileName = Path.GetFileName(x.LocalFilePath),
                }).ToList() : new List<AssetReferenceParams>(),
                StartMultiplayerServerCommand = settings.ProcessStartParameters.StartGameCommand,
                OsPlatform = Globals.GameServerEnvironment.ToString(),
                Metadata = (Dictionary<string, string>)settings.DeploymentMetadata ?? new Dictionary<string, string>()
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
            do
            {
                var ContainerImagesRequest = new ListContainerImagesRequest
                {
                    SkipToken = imageSkipToken,
                    PageSize = pageSize
                };
                var ContainerImagesResponse = await PlayFabMultiplayerAPI.ListContainerImagesAsync(ContainerImagesRequest);

                ErrorCheck(ContainerImagesResponse.Error);
                imageSkipToken = ContainerImagesResponse.Result.SkipToken ?? null;
                IEnumerable<string> existingImages = ContainerImagesResponse.Result.Images.Where(x => x == settings.ContainerStartParameters.ImageDetails.ImageName);
                if (!existingImages.Any() && string.IsNullOrEmpty(imageSkipToken))
                {
                    //TODO: upload LinuxContainer image API
                    //This is currently non-existent 
                    Console.WriteLine("Make sure you have uploaded your Linux container image to Docker before attempting deploy");
                }
                
            } while (!string.IsNullOrEmpty(imageSkipToken));
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
            List<string> failedCertUploads = new List<string>();

            do
            {
                var certificateSummariesRequest = new ListCertificateSummariesRequest
                {
                    SkipToken = certSkipToken,
                    PageSize = pageSize
                };
                var certificateSummariesResponse = await PlayFabMultiplayerAPI.ListCertificateSummariesAsync(certificateSummariesRequest);

                ErrorCheck(certificateSummariesResponse.Error);

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
                
            } while (!string.IsNullOrEmpty(certSkipToken));

            if (failedCertUploads?.Count > 0)
            {
                Console.WriteLine($"The folowing certificates failed in the upload process: {string.Join(", ", failedCertUploads)}");
            }
        }

        public List<PlayFab.MultiplayerModels.Port> PortMapping()
        {
            var ports = new List<PlayFab.MultiplayerModels.Port>();

            foreach (var portList in settings.PortMappingsList)
            {
                ports.AddRange(portList?.Select(x => new PlayFab.MultiplayerModels.Port()
                {
                    Name = x.GamePort.Name,
                    Num = settings.RunContainer ? x.GamePort.Number : 0,
                    Protocol = Enum.Parse<ProtocolType>(x.GamePort.Protocol)
                }).ToList());
            }

            return ports;
        }

        public async Task UploadAssetFileAsync(string uploadUrl, string fileNamePath)
        {
            string fileName = Path.GetFileName(fileNamePath);
            try
            {
                //TODO: log progress of asset upload
                var uri = new Uri(uploadUrl);
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

        public async Task CheckAndUploadAssetFileAsync(string fileNamePath)
        {
            string fileName = Path.GetFileName(fileNamePath);
            GetAssetUploadUrlRequest uriRequest = new GetAssetUploadUrlRequest() { FileName = fileName };
            var uriResult = await PlayFabMultiplayerAPI.GetAssetUploadUrlAsync(uriRequest);
            
            if (uriResult.Error != null)
            {
                if (uriResult.Error.ErrorMessage.Contains("AssetAlreadyExists"))
                {
                    Console.WriteLine($"{fileName} is already uploaded. " +
                        $"\nIf this is a mistake in file naming, enter 'Y'. This will allow you to end the current session so you can rename your file and run again" +
                        $"\nEnter 'N' if you want to go ahead.");
                    
                    string msg = Console.ReadLine();
                    if (msg.ToUpper() == "Y")
                    {
                        Environment.Exit(1);
                    }
                    else if (msg.ToUpper() == "N")
                    {
                        Console.WriteLine("Going ahead with deployment...");
                    }
                }
                else
                {
                    Console.WriteLine(uriResult.Error.ErrorMessage);
                    Environment.Exit(1);   
                }
            }
            else
            {
                await UploadAssetFileAsync(uriResult.Result.AssetUploadUrl, fileNamePath);
            }
        }

        public void ErrorCheck(PlayFabError err)
        {
            if (err != null && err.ErrorMessage != null)
            {
                Console.WriteLine($"{err.ErrorMessage}");
                Environment.Exit(1);
            }
        }
    }
}