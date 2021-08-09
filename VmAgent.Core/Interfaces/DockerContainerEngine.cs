// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.ContainerEngines
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using Microsoft.Azure.Gaming.VmAgent.Model;

    using Version = System.Version;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Core;
    using Core.Interfaces;

    using Microsoft.Azure.Gaming.AgentInterfaces;
    using Polly;

    public class DockerContainerEngine : BaseSessionHostRunner
    {
        private const string DockerWindowsNamedPipe = "npipe://./pipe/docker_engine";

        private const string DockerUnixDomainSocket = "unix:///var/run/docker.sock";

        private const string DockerApiVersion = "1.25";
        private static readonly ContainerStartParameters DefaultStartParameters = new ContainerStartParameters();

        private readonly double _createImageRetryTimeMins = 5.0;

        private readonly int _maxRetryAttempts = 30;

        /// <summary>
        /// The name of the file where the console logs for the server are captured.
        /// </summary>
        private const string ConsoleLogCaptureFileName = "PF_ConsoleLogs.txt";

        /// <summary>
        /// Lazily instantiates the Docker client.
        /// </summary>
        private readonly IDockerClient _dockerClient;

        public DockerContainerEngine(
            VmConfiguration vmConfiguration,
            MultiLogger logger,
            Core.Interfaces.ISystemOperations systemOperations,
            IDockerClient dockerClient = null)
            : base (vmConfiguration, logger, systemOperations)
        {
            _dockerClient = dockerClient ?? CreateDockerClient();
        }

        private IDockerClient CreateDockerClient()
        {
            DockerClientConfiguration dockerConfig =
                new DockerClientConfiguration(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? new Uri(DockerWindowsNamedPipe)
                        : new Uri(DockerUnixDomainSocket));
            return dockerConfig.CreateClient(new Version(DockerApiVersion));
        }

        /// <summary>
        /// Gets the IP address that's associated with the host's network interface within the Docker created network.
        /// </summary>
        /// <returns>
        /// The IP address that's associated with the host's network interface within the Docker created network.
        /// </returns>
        /// <remarks>
        /// The Docker network used is created by the startup script that kicks off the Agent.
        /// This IP address is used to listen to http requests (such as heartbeats) coming from the Game SDK
        /// running within the containers.
        /// Sample network configuration (linux):
        /// [
        ///    {
        ///        "Name": "bridge",
        ///        "Id": "431652b041847c2dba2c0407bff7de5b52782c6331807ab3444c18d6e1b923b4",
        ///        "Scope": "local",
        ///        "Driver": "bridge",
        ///        "EnableIPv6": false,
        ///        "IPAM": {
        ///            "Driver": "default",
        ///            "Options": null,
        ///            "Config": [
        ///                {
        ///                    "Subnet": "172.17.0.0/16",
        ///                    "Gateway": "172.17.0.1"
        ///                }
        ///            ]
        ///        },
        ///        "Internal": false,
        ///        "Containers": {},
        ///        "Options": {
        ///            "com.docker.network.bridge.default_bridge": "true",
        ///            "com.docker.network.bridge.enable_icc": "true",
        ///            "com.docker.network.bridge.enable_ip_masquerade": "true",
        ///            "com.docker.network.bridge.host_binding_ipv4": "0.0.0.0",
        ///            "com.docker.network.bridge.name": "docker0",
        ///            "com.docker.network.driver.mtu": "1500"
        ///        },
        ///        "Labels": {}
        ///    }
        /// ]
        /// </remarks>
        public override string GetVmAgentIpAddress()
        {
            try
            {
                return _dockerClient.Networks.ListNetworksAsync()
                        .Result.Single(
                            x => string.Equals(SessionHostContainerConfiguration.DockerNetworkName, x.Name,
                                StringComparison.OrdinalIgnoreCase))
                        .IPAM.Config.Single().Gateway;

            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Failed to find network '{SessionHostContainerConfiguration.DockerNetworkName}'. Ensure you have properly setup networking via setup.ps1",
                    e);
            }
        }

        public override async Task WaitOnServerExit(string containerId)
        {
            ContainerWaitResponse containerWaitResponse = await _dockerClient.Containers.WaitContainerAsync(containerId).ConfigureAwait(false);
            _logger.LogInformation($"Container {containerId} exited with exit code {containerWaitResponse.StatusCode}.");
        }

        private async Task<string> CreateContainer(
            string imageName,
            IList<string> environmentVariables,
            IList<string> volumeBindings,
            IList<PortMapping> portMappings,
            IList<string> startCmd,
            HostConfig hostConfigOverrides,
            string workingDirectory)
        {
            HostConfig hostConfig = hostConfigOverrides ?? new HostConfig();
            hostConfig.NetworkMode = SessionHostContainerConfiguration.DockerNetworkName;
            hostConfig.Binds = volumeBindings;
            hostConfig.PortBindings = portMappings?.ToDictionary(p => $"{p.GamePort.Number}/{p.GamePort.Protocol}",
                p => (IList<PortBinding>)(new List<PortBinding>() { new PortBinding() { HostPort = $"{p.NodePort}/{p.GamePort.Protocol}" } }));

            if(hostConfig.LogConfig == null)
            {
                hostConfig.LogConfig = new LogConfig();
                hostConfig.LogConfig.Type = "json-file";
                hostConfig.LogConfig.Config = new Dictionary<string, string>() { { "max-size", "200m" } };
            }

            CreateContainerParameters containerParams = new CreateContainerParameters
            {
                Image = imageName,
                Env = environmentVariables,
                ExposedPorts = portMappings?.ToDictionary(p => $"{p.GamePort.Number}/{p.GamePort.Protocol}", p => new EmptyStruct()),
                HostConfig = hostConfig,
                WorkingDir = workingDirectory,
                Cmd = startCmd
            };

            _logger.LogInformation($"Creating container. Image='{imageName}'");
            CreateContainerResponse response =
                await TaskUtil.TimedExecute(
                    async () => await _dockerClient.Containers.CreateContainerAsync(containerParams).ConfigureAwait(false),
                    _logger,
                    MetricConstants.ContainerStats,
                    MetricConstants.ContainerCreationTime);
            _logger.LogInformation($"Created a container with session host id: {response.ID}");

            return response.ID;
        }

        private async Task StartContainer(string containerId)
        {
            await TaskUtil.TimedExecute(
                async () => await _dockerClient.Containers.StartContainerAsync(containerId, DefaultStartParameters),
                _logger,
                MetricConstants.ContainerStats,
                MetricConstants.ContainerStartTime);
            _logger.LogInformation($"Container {containerId} start completed.");
        }
        
        private class LogReporter : IProgress<JSONMessage>
        {
            private readonly ConcurrentDictionary<string, LayerProgress> _layerProgresses = new ConcurrentDictionary<string, LayerProgress>();

            private readonly MultiLogger _logger;
            internal LogReporter(MultiLogger logger)
            {
                _logger = logger;
            }
            public void Report(JSONMessage value)
            {
                DateTime utcNow = DateTime.UtcNow;
                if (value == null)
                {
                    return;
                }
                switch (value.Status?.ToLowerInvariant())
                {
                    case "downloading":
                        {
                            LayerProgress layer = _layerProgresses.GetOrAdd(value.ID, new LayerProgress { DownloadStartTimestamp = utcNow });
                            layer.LayerSize = value.Progress?.Total;
                            break;
                        }
                    case "download complete":
                        {
                            LayerProgress layer = _layerProgresses.GetOrAdd(value.ID, new LayerProgress { DownloadStartTimestamp = utcNow });
                            layer.DownloadCompletionTimestamp = utcNow;
                            break;
                        }
                    case "extracting":
                        {
                            if (_layerProgresses.TryGetValue(value.ID, out LayerProgress layer))
                            {
                                layer.ExtractionStartTimestamp = utcNow;
                            }
                            break;
                        }
                    case "pull complete":
                        {
                            if (_layerProgresses.TryGetValue(value.ID, out LayerProgress layer))
                            {
                                layer.PullCompletionTimestamp = utcNow;
                            }
                            break;
                        }
                    default:
                        break;
                }

                _logger.LogVerbose(value.ToJsonString());

            }

            public OperationSummary ExtractionSummary
            {
                get
                {
                    List<LayerProgress> values = _layerProgresses.Values.Where(x => x.ExtractionDurationMs > 0).ToList();
                    if (values.All(x => x.LayerSize.HasValue))
                    {
                        return new OperationSummary(values.Count, values.Select(x => x.ExtractionDurationMs).Sum(), values.Select(x => x.LayerSize.Value).Sum());
                    }

                    return null;
                }
            }

            public OperationSummary DownloadSummary
            {
                get
                {
                    List<LayerProgress> values = _layerProgresses.Values.Where(x => x.DownloadDurationMs > 0).ToList();
                    if (values.All(x => x.LayerSize.HasValue))
                    {
                        return new OperationSummary(values.Count, values.Select(x => x.DownloadDurationMs).Sum(), values.Select(x => x.LayerSize.Value).Sum());
                    }

                    return null;
                }
            }

            private class LayerProgress
            {
                private DateTime? _extractionStartTimestamp;
                private DateTime? _pullCompletionTimestamp;
                public DateTime? DownloadStartTimestamp { private get; set; }
                public DateTime? DownloadCompletionTimestamp { private get; set; }

                public DateTime? ExtractionStartTimestamp
                {
                    private get
                    {
                        return _extractionStartTimestamp;
                    }
                    set
                    {
                        _extractionStartTimestamp = _extractionStartTimestamp ?? value;
                    }
                }
                public DateTime? PullCompletionTimestamp
                {
                    private get
                    {
                        return _pullCompletionTimestamp;
                    }
                    set
                    {
                        _pullCompletionTimestamp = _pullCompletionTimestamp ?? value;
                    }
                }

                public long DownloadDurationMs
                {
                    get
                    {
                        return (long)(DownloadCompletionTimestamp - DownloadStartTimestamp).GetValueOrDefault().TotalMilliseconds;
                    }
                }

                public long ExtractionDurationMs
                {
                    get
                    {
                        return (long)(PullCompletionTimestamp - ExtractionStartTimestamp).GetValueOrDefault().TotalMilliseconds;
                    }
                }

                public long? LayerSize { get; set; }
            }
        }

        private class OperationSummary
        {
            public int LayerCount { get; private set; }
            public long DurationInMilliseconds { get; private set; }
            public long TotalSizeInBytes { get; private set; }

            public OperationSummary(int layerCount, long durationInMilliseconds, long totalSizeInBytes)
            {
                LayerCount = layerCount;
                DurationInMilliseconds = durationInMilliseconds;
                TotalSizeInBytes = totalSizeInBytes;
            }
        }

        /// <summary>
        /// Creates and starts a container, assigning <paramref name="instanceNumber"/> to it.
        /// </summary>
        /// <param name="instanceNumber">
        /// An instance number associated with a container. It is used to map assets folder to the container
        /// and then re-use for container recycling.
        /// </param>
        /// <returns>A <see cref="Task"/>.</returns>
        public override async Task<SessionHostInfo> CreateAndStart(int instanceNumber, GameResourceDetails gameResourceDetails, ISessionHostManager sessionHostManager)
        {
            // The current Docker client doesn't yet allow specifying a local name for the image.
            // It is stored with as the remote path name. Thus, the parameter to CreateAndStartContainers
            // is the same as the remote image path.
            SessionHostsStartInfo sessionHostStartInfo = gameResourceDetails.SessionHostsStartInfo;
            ContainerImageDetails imageDetails = sessionHostStartInfo.ImageDetails;
            string imageName = $"{imageDetails.ImageName}:{imageDetails.ImageTag ?? "latest"}";
            
            // Support running local images with no explicit registry in the name.
            if (imageDetails.Registry.Length > 0)
            {
                imageName = $"{imageDetails.Registry}/{imageName}";
            }

            // The game containers need a unique folder to write their logs. Ideally,
            // we would specify the containerId itself as the subfolder. However, we have to
            // specify volume bindings before docker gives us the container id, so using
            // a random guid here instead
            string logFolderId = _systemOperations.NewGuid().ToString("D");
            ISessionHostConfiguration sessionHostConfiguration = new SessionHostContainerConfiguration(_vmConfiguration, _logger, _systemOperations, _dockerClient, sessionHostStartInfo);
            IList<PortMapping> portMappings = sessionHostConfiguration.GetPortMappings(instanceNumber);
            List<string> environmentValues = sessionHostConfiguration.GetEnvironmentVariablesForSessionHost(instanceNumber, logFolderId, sessionHostManager.VmAgentSettings)
                .Select(x => $"{x.Key}={x.Value}").ToList();

            string dockerId = await CreateContainer(
                imageName,
                environmentValues,
                GetVolumeBindings(sessionHostStartInfo, instanceNumber, logFolderId, sessionHostManager.VmAgentSettings),
                portMappings,
                GetStartGameCmd(sessionHostStartInfo),
                sessionHostStartInfo.HostConfigOverrides,
                GetGameWorkingDir(sessionHostStartInfo));

            SessionHostInfo sessionHost = sessionHostManager.AddNewSessionHost(dockerId, sessionHostStartInfo.AssignmentId, instanceNumber, logFolderId);

            // https://docs.docker.com/docker-for-windows/networking/
            string agentIPaddress = sessionHostManager.LinuxContainersOnWindows ? "host.docker.internal" : GetVmAgentIpAddress();

            sessionHostConfiguration.Create(instanceNumber, dockerId, agentIPaddress, _vmConfiguration, logFolderId);

            // on LinuxContainersForWindows, VMAgent will run in a Windows environment 
            // but we want the Linux directory separator char
            if (sessionHostManager.LinuxContainersOnWindows)
            {
                string configFilePath = Path.Combine(_vmConfiguration.GetConfigRootFolderForSessionHost(instanceNumber),
                    VmDirectories.GsdkConfigFilename);
                File.WriteAllText(configFilePath, File.ReadAllText(configFilePath).
                    Replace($"{_vmConfiguration.VmDirectories.GameLogsRootFolderContainer}\\\\",
                        $"{_vmConfiguration.VmDirectories.GameLogsRootFolderContainer}/"));
            }
            try
            {
                await StartContainer(dockerId);
                _logger.LogInformation($"Started container {dockerId}, with assignmentId {sessionHostStartInfo.AssignmentId}, instance number {instanceNumber}, and logFolderId {logFolderId}");
            } 
            catch (Exception exception)
            {
                _logger.LogException($"Failed to start container based host with instance number {instanceNumber}", exception);
                sessionHostManager.RemoveSessionHost(dockerId);
                sessionHost = null;
            }
           

            return sessionHost;
        }

        private IList<string> GetVolumeBindings(SessionHostsStartInfo request, int sessionHostInstance, string logFolderId, VmAgentSettings agentSettings)
        {
            List<string> volumeBindings = new List<string>();
            if (request.AssetDetails?.Length > 0)
            {
                for (int i = 0; i < request.AssetDetails.Length; i++)
                {
                    string pathOnHost = request.UseReadOnlyAssets ? _vmConfiguration.GetAssetExtractionFolderPathForSessionHost(0, i) :
                    _vmConfiguration.GetAssetExtractionFolderPathForSessionHost(sessionHostInstance, i);

                    volumeBindings.Add($"{pathOnHost}:{request.AssetDetails[i].MountPath}");
                }
            }

            // The folder needs to exist before the mount can happen.
            string logFolderPathOnVm = Path.Combine(_vmConfiguration.VmDirectories.GameLogsRootFolderVm, logFolderId);
            _systemOperations.CreateDirectory(logFolderPathOnVm);

            if (agentSettings.EnableCrashDumpProcessing)
            {
                // Create the dumps folder as a subfolder of the logs folder
                string dumpFolderPathOnVm = Path.Combine(logFolderPathOnVm, VmDirectories.GameDumpsFolderName);
                _systemOperations.CreateDirectory(dumpFolderPathOnVm);
            }

            // Set up the log folder. Maps D:\GameLogs\{logFolderId} on the container host to C:\GameLogs on the container.
            // TODO: TBD whether the log folder should be taken as input from developer during ingestion.
            volumeBindings.Add($"{logFolderPathOnVm}:{_vmConfiguration.VmDirectories.GameLogsRootFolderContainer}");

            // All containers will have the certificate folder mapped
            volumeBindings.Add($"{_vmConfiguration.VmDirectories.CertificateRootFolderVm}:{_vmConfiguration.VmDirectories.CertificateRootFolderContainer}");

            // All containers have the same shared content folder mapped.
            _systemOperations.CreateDirectory(_vmConfiguration.VmDirectories.GameSharedContentFolderVm);
            volumeBindings.Add($"{_vmConfiguration.VmDirectories.GameSharedContentFolderVm}:{_vmConfiguration.VmDirectories.GameSharedContentFolderContainer}");

            // Map the folder that will contain this session host's configuration file
            string configFolderPathOnVm = _vmConfiguration.GetConfigRootFolderForSessionHost(sessionHostInstance);
            _systemOperations.CreateDirectory(configFolderPathOnVm);
            volumeBindings.Add($"{configFolderPathOnVm}:{_vmConfiguration.VmDirectories.GsdkConfigRootFolderContainer}");

            return volumeBindings;
        }

        public override async Task CollectLogs(string id, string logsFolder, ISessionHostManager sessionHostManager)
        {
            try
            {
                _logger.LogVerbose($"Collecting logs for container {id}.");
                string destinationFileName = Path.Combine(logsFolder, ConsoleLogCaptureFileName);

                if (sessionHostManager.LinuxContainersOnWindows)
                {
                    // we do this for lcow since containers are running on a Hyper-V Linux machine
                    // which the host Windows machine does not have "copy" access to, to get the logs with a FileCopy
                    // this is only supposed to run on LocalMultiplayerAgent running on lcow
                    StringBuilder sb = new StringBuilder();
                    Stream logsStream = await _dockerClient.Containers.GetContainerLogsAsync(id,
                        new ContainerLogsParameters() {ShowStdout = true, ShowStderr = true});
                    using (StreamReader sr = new StreamReader(logsStream))
                    {
                        Stopwatch sw = new Stopwatch();
                        while (!sr.EndOfStream)
                        {
                            if (sw.Elapsed.Seconds > 3) // don't flood STDOUT with messages, output one every 3 seconds if logs are too many
                            {
                                _logger.LogVerbose($"Gathering logs for container {id}, please wait...");
                                sw.Restart();
                            }
                            _systemOperations.FileAppendAllText(destinationFileName, sr.ReadLine() + Environment.NewLine);
                        }
                    }
                    _logger.LogVerbose($"Written logs for container {id} to {destinationFileName}.");
                }
                else
                {
                    ContainerInspectResponse containerInspectResponse = await _dockerClient.Containers.InspectContainerAsync(id);
                    string dockerLogsPath = containerInspectResponse?.LogPath;
                    if (!string.IsNullOrEmpty(dockerLogsPath) && _systemOperations.FileExists(dockerLogsPath))
                    {
                        _logger.LogVerbose($"Copying log file {dockerLogsPath} for container {id} to {destinationFileName}.");
                        _systemOperations.FileCopy(dockerLogsPath, destinationFileName);
                    }
                }
            }
            catch (DockerContainerNotFoundException)
            {
                _logger.LogInformation($"Docker container {id} not found.");
            }
        }

        public override async Task<bool> TryDelete(string id)
        {
            bool result = true;
            try
            {
                _logger.LogInformation($"Deleting container {id}.");
                await _dockerClient.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters() { Force = true });
            }
            catch (DockerContainerNotFoundException)
            {
                _logger.LogInformation($"Docker container {id} not found.");
            }
            catch (Exception exception)
            {
                _logger.LogException(exception);
                result = false;
            }

            return result;
        }

        public override async Task DeleteResources(SessionHostsStartInfo sessionHostsStartInfo)
        {
            ContainerImageDetails imageDetails = sessionHostsStartInfo.ImageDetails;
            string imageName = $"{imageDetails.Registry}/{imageDetails.ImageName}:{imageDetails.ImageTag ?? "latest"}";
            _logger.LogInformation($"Starting deletion of container image {imageName}");
            try
            {
                await _dockerClient.Images.DeleteImageAsync(imageName, new ImageDeleteParameters() { Force = true });
                _logger.LogInformation($"Deleted container image {imageName}");
            }
            catch (DockerImageNotFoundException)
            {
                _logger.LogInformation($"Image {imageName} not found.");
            }
        }

        public override async Task RetrieveResources(SessionHostsStartInfo sessionHostsStartInfo)
        {
            string registryWithImageName = $"{sessionHostsStartInfo.ImageDetails.Registry}/{sessionHostsStartInfo.ImageDetails.ImageName}";
            string imageTag = sessionHostsStartInfo.ImageDetails.ImageTag;
            string username = sessionHostsStartInfo.ImageDetails.Username;
            string password = sessionHostsStartInfo.ImageDetails.Password;
            if (string.IsNullOrEmpty(imageTag))
            {
                imageTag = "latest";
            }

            _logger.LogInformation($"Starting image pull for: {registryWithImageName}:{imageTag}.");
            LogReporter logReporter = new LogReporter(_logger);

            Polly.Retry.AsyncRetryPolicy retryPolicy = Policy
                .Handle<Exception>((Exception e) =>
                {
                    _logger.LogError($"Exception encountered when creating image: {e.ToString()}");
                    return true;
                })
                .WaitAndRetryAsync(_maxRetryAttempts, i => TimeSpan.FromMinutes(_createImageRetryTimeMins / _maxRetryAttempts));

            await retryPolicy.ExecuteAsync(async () =>
            {
                await _dockerClient.Images.CreateImageAsync(
                    new ImagesCreateParameters {FromImage = registryWithImageName, Tag = imageTag},
                    new AuthConfig() {Username = username, Password = password},
                    logReporter);

                // Making sure that the image was actually downloaded properly
                // We have seen some cases where Docker Registry API returns 'success' on pull while the image has not been properly downloaded
                IEnumerable<ImagesListResponse> images = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters { All = true });
                if (images.All(image => !image.RepoTags.Contains($"{registryWithImageName}:{imageTag}")))
                {
                    throw new ApplicationException("CreateImageAsync is completed but the image doesn't exist");
                }
            });
            
            _logger.LogEvent(MetricConstants.PullImage, null, new Dictionary<string, double>
                {
                    { MetricConstants.DownloadDurationInMilliseconds, logReporter.DownloadSummary?.DurationInMilliseconds ?? 0d },
                    { MetricConstants.ExtractDurationInMilliseconds,  logReporter.ExtractionSummary?.DurationInMilliseconds ?? 0d },
                    { MetricConstants.SizeInBytes, logReporter.DownloadSummary?.TotalSizeInBytes ?? 0d }
                }
            );
        }

        public override async Task<IEnumerable<string>> List()
        {
            return (await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters())).Select(x => x.ID);
        }

        /// <summary>
        /// Creates a list whose first element contains
        /// the command we want to use to run the game
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private IList<string> GetStartGameCmd(SessionHostsStartInfo request)
        {
            if (!string.IsNullOrEmpty(request.StartGameCommand))
            {
                if (_systemOperations.IsOSPlatform(OSPlatform.Windows))
                {
                    return new List<string>() { $"cmd /c {request.StartGameCommand}" };
                }

                return new List<string>() { request.StartGameCommand };
            }

            return null;
        }

        /// <summary>
        /// Gets the game working directory based off the
        /// command we want to use to run the game
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string GetGameWorkingDir(SessionHostsStartInfo request)
        {
            // Only set the working directory for managed containers (aka. Windows)
            if (_systemOperations.IsOSPlatform(OSPlatform.Windows))
            {
                if (!string.IsNullOrEmpty(request.GameWorkingDirectory))
                {
                    _logger.LogInformation($"Container working dir set from GameWorkingDirectory: {request.GameWorkingDirectory}");
                    return request.GameWorkingDirectory;
                }

                if (!string.IsNullOrEmpty(request.StartGameCommand))
                {
                    // Strip off any arguments that might come after the game executable
                    string executable = request.StartGameCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

                    // Return the path to the folder where the executable lives, note that this returns the full path
                    return Path.GetDirectoryName(executable);
                }
            }
            return null;
        }
    }
}
