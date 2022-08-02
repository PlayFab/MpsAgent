using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Azure.Gaming.LocalMultiplayerAgent.DeploymentTool;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Gaming.AgentInterfaces;
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
            Console.WriteLine("Deploying to PlayFab Multiplayer Servers...\n");

            var auth = await PlayFabAuthentication();

            ErrorCheck(auth.Error);

            DeploymentSettingsValidator validator = new DeploymentSettingsValidator(deploymentSettings);

            if (!validator.IsValid())
            {
                Console.WriteLine("The specified settings are invalid. Please correct them and re-run the agent.");
                Environment.Exit(1);
            }

            if (settings.GameCertificateDetails != null)
            {
                await CheckAndUploadCertificatesAsync();
            }

            if (settings.AssetDetails != null)
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
                if (createBuild.Error.ErrorMessage != null)
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
            PlayFabSettings.staticSettings.TitleId = settings.TitleId;
            var authValidation = new PlayFabResult<PlayFab.AuthenticationModels.GetEntityTokenResponse>();
            string secret = Environment.GetEnvironmentVariable("PF_SECRET", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(secret))
            {
                Console.WriteLine("Enter developer secret key");
                secret = Console.ReadLine();
            }

            PlayFabSettings.staticSettings.DeveloperSecretKey = secret;

            try
            {
                var tokenReq = new PlayFab.AuthenticationModels.GetEntityTokenRequest();
                authValidation = await PlayFabAuthenticationAPI.GetEntityTokenAsync(tokenReq);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }

            return authValidation;
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
                GameCertificateReferences = settings.GameCertificateDetails?.Select(x => new GameCertificateReferenceParams()
                {
                    Name = x.Name
                }).ToList() ?? new List<GameCertificateReferenceParams>(),
                ContainerRunCommand = settings.ContainerStartParameters.StartGameCommand,
                AreAssetsReadonly = deploymentSettings.AreAssetsReadonly,
                RegionConfigurations =deploymentSettings.RegionConfigurations?.Select(x => new BuildRegionParams()
                {
                    Region = x.Region,
                    MaxServers = x.MaxServers,
                    StandbyServers = x.StandbyServers,
                    MultiplayerServerCountPerVm = x.MultiplayerServerCountPerVm,
                    VmSize = x.VmSize

                }).ToList() ?? new List<BuildRegionParams>(),
                GameAssetReferences = settings.AssetDetails?.Select(x => new AssetReferenceParams()
                {
                    FileName = Path.GetFileName(x.LocalFilePath),
                    MountPath = x.MountPath
                }).ToList() ?? new List<AssetReferenceParams>(),
                MultiplayerServerCountPerVm = deploymentSettings.MultiplayerServerCountPerVm,
                Metadata = (Dictionary<string, string>)settings.DeploymentMetadata ?? new Dictionary<string, string>()
            };
        }

        public CreateBuildWithManagedContainerRequest GetManagedContainerRequest()
        {
            return new CreateBuildWithManagedContainerRequest
            {
                VmSize = Enum.Parse<AzureVmSize>(deploymentSettings.VmSize),
                GameCertificateReferences = settings.GameCertificateDetails?.Select(x => new GameCertificateReferenceParams()
                {
                    Name = x.Name
                }).ToList() ?? new List<GameCertificateReferenceParams>(),
                Ports = PortMapping(),
                MultiplayerServerCountPerVm = deploymentSettings.MultiplayerServerCountPerVm,
                AreAssetsReadonly = deploymentSettings.AreAssetsReadonly,
                RegionConfigurations = deploymentSettings.RegionConfigurations?.Select(x => new BuildRegionParams()
                {
                    Region = x.Region,
                    MaxServers = x.MaxServers,
                    StandbyServers = x.StandbyServers,
                    MultiplayerServerCountPerVm = x.MultiplayerServerCountPerVm,
                    VmSize = x.VmSize
                }).ToList() ?? new List<BuildRegionParams>(),
                BuildName = deploymentSettings.BuildName,
                GameAssetReferences = settings.AssetDetails.Select(x => new AssetReferenceParams()
                {
                    FileName = Path.GetFileName(x.LocalFilePath),
                    MountPath = x.MountPath
                }).ToList() ?? new List<AssetReferenceParams>(),
                StartMultiplayerServerCommand = settings.ContainerStartParameters.StartGameCommand,
                ContainerFlavor = ContainerFlavor.ManagedWindowsServerCore,
                WindowsCrashDumpConfiguration = deploymentSettings.WindowsCrashDumpConfiguration,
                Metadata = (Dictionary<string, string>)settings.DeploymentMetadata ?? new Dictionary<string, string>()
            };
        }

        public CreateBuildWithProcessBasedServerRequest GetProcessBasedServerRequest()
        {
            return new CreateBuildWithProcessBasedServerRequest
            {
                VmSize = Enum.Parse<AzureVmSize>(deploymentSettings.VmSize),
                GameCertificateReferences = settings.GameCertificateDetails?.Select(x => new GameCertificateReferenceParams()
                {
                    Name = x.Name
                }).ToList() ?? new List<GameCertificateReferenceParams>(),
                Ports = PortMapping(),
                MultiplayerServerCountPerVm = deploymentSettings.MultiplayerServerCountPerVm,
                AreAssetsReadonly = deploymentSettings.AreAssetsReadonly,
                RegionConfigurations = deploymentSettings.RegionConfigurations?.Select(x => new BuildRegionParams()
                {
                    Region = x.Region,
                    MaxServers = x.MaxServers,
                    StandbyServers = x.StandbyServers,
                    MultiplayerServerCountPerVm = x.MultiplayerServerCountPerVm,
                    VmSize = x.VmSize

                }).ToList() ?? new List<BuildRegionParams>(),
                BuildName = deploymentSettings.BuildName,
                GameAssetReferences = settings.AssetDetails.Select(x => new AssetReferenceParams()
                {
                    FileName = Path.GetFileName(x.LocalFilePath),
                }).ToList() ?? new List<AssetReferenceParams>(),
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

        public async Task UploadLinuxContainerImageAsync(ContainerImageDetails imageDetails)
        {
            GetContainerRegistryCredentialsRequest requ = new GetContainerRegistryCredentialsRequest();
            var ContainerRegistryCredentialsRes = await PlayFabMultiplayerAPI.GetContainerRegistryCredentialsAsync(requ);
            ErrorCheck(ContainerRegistryCredentialsRes.Error);

            try
            {
                DockerClient client = new DockerClientConfiguration().CreateClient();
                string registryWithImageName = $"{imageDetails.Registry}/{imageDetails.ImageName}";
                Progress<JSONMessage> theMess = new Progress<JSONMessage>(
                    msg =>
                    {    
                        Console.WriteLine($"{msg.Status}|{msg.ProgressMessage}|{msg.ErrorMessage}");
                    });

                await client.Images.PushImageAsync(
                    registryWithImageName,
                    new ImagePushParameters
                    {
                        Tag = imageDetails.ImageTag,
                    },
                    new AuthConfig
                    {
                        Username = ContainerRegistryCredentialsRes.Result.Username,
                        Password = ContainerRegistryCredentialsRes.Result.Password
                    },
                    theMess
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
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
                IEnumerable<string> existingImageNames = ContainerImagesResponse.Result.Images.Where(x => x == settings.ContainerStartParameters.ImageDetails.ImageName);

                if (!existingImageNames.Any() && string.IsNullOrEmpty(imageSkipToken))
                {
                    await UploadLinuxContainerImageAsync(settings.ContainerStartParameters.ImageDetails);
                }
                else if (existingImageNames.Any())
                {
                    var ContainerImageTagsRequest = new ListContainerImageTagsRequest
                    {
                        ImageName = existingImageNames.First() ?? null,
                    };
                    var ContainerImageTagsResponse = await PlayFabMultiplayerAPI.ListContainerImageTagsAsync(ContainerImageTagsRequest);

                    ErrorCheck(ContainerImageTagsResponse.Error);

                    IEnumerable<string> existingImageTags = ContainerImageTagsResponse.Result.Tags.Where(x => x == settings.ContainerStartParameters.ImageDetails.ImageTag);

                    if (existingImageTags.Any())
                    {
                        Console.WriteLine("Your container image is already uploaded. Continuing deployment...");
                        break;
                    }

                    if (!existingImageTags.Any() && string.IsNullOrEmpty(imageSkipToken))
                    {
                        await UploadLinuxContainerImageAsync(settings.ContainerStartParameters.ImageDetails);
                    }
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