// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using AgentInterfaces;
    using Config;
    using Microsoft.Azure.Gaming.VmAgent.Core.Dependencies;
    using VmAgent.Core;
    using VmAgent.Core.Interfaces;
    using VmAgent.Model;

    public class MultiplayerServerManager
    {
        private readonly ISystemOperations _systemOperations;
        private readonly ISessionHostRunnerFactory _sessionHostRunnerFactory;
        private readonly MultiLogger _logger;
        private readonly VmConfiguration _vmConfiguration;
        private readonly BasicAssetExtractor _basicAssetExtractor;

        public MultiplayerServerManager(
            ISystemOperations systemOperations,
            VmConfiguration vmConfiguration,
            MultiLogger logger,
            ISessionHostRunnerFactory sessionHostRunnerFactory,
            BasicAssetExtractor basicAssetExtractor = null)
        {
            _sessionHostRunnerFactory = sessionHostRunnerFactory;
            _systemOperations = systemOperations;
            _logger = logger;
            _vmConfiguration = vmConfiguration;
            _basicAssetExtractor = new BasicAssetExtractor(_systemOperations, _logger);
        }

        public async Task CreateAndStartContainerWaitForExit(SessionHostsStartInfo startParameters)
        {
            IList<PortMapping> portMappings = GetPortMappings(startParameters, 0);

            if (startParameters.SessionHostType == SessionHostType.Container)
            {
                foreach (PortMapping mapping in portMappings)
                {
                    _logger.LogInformation(
                        $"{mapping.GamePort.Name} ({mapping.GamePort.Protocol}): Local port {mapping.NodePort} mapped to container port {mapping.GamePort.Number} ");
                }
            }

            DownloadAndExtractAllAssets(startParameters);
            DownloadGameCertificates(startParameters);

            ISessionHostRunner sessionHostRunner =
                _sessionHostRunnerFactory.CreateSessionHostRunner(startParameters.SessionHostType, _vmConfiguration, _logger);

            // RetrieveResources does a docker pull.
            // Windows game servers always pull from the registry.
            // For Linux containers on Windows (LCOW), set ForcePullFromAcrOnLinuxContainersOnWindows to true to pull.
            // For macOS/Linux, set ForcePullContainerImageFromRegistry to true to pull from a remote registry.
            // If your image is locally built (e.g. via "docker build"), leave both flags false to skip the pull.
            if (Globals.GameServerEnvironment == GameServerEnvironment.Windows
                || Globals.Settings.ForcePullFromAcrOnLinuxContainersOnWindows
                || Globals.Settings.ForcePullContainerImageFromRegistry)
            {
                await sessionHostRunner.RetrieveResources(startParameters);
            }

            NoOpSessionHostManager sessionHostManager = new NoOpSessionHostManager();
            SessionHostInfo sessionHostInfo =
                await sessionHostRunner.CreateAndStart(0, new GameResourceDetails { SessionHostsStartInfo = startParameters }, sessionHostManager);
            if (sessionHostInfo == null)
            {
                return;
            }

            string typeSpecificId = sessionHostInfo.TypeSpecificId;
            
            _logger.LogInformation("Waiting for heartbeats from the game server.....");

            await sessionHostRunner.WaitOnServerExit(typeSpecificId).ConfigureAwait(false);
            string logFolder = Path.Combine(Globals.VmConfiguration.VmDirectories.GameLogsRootFolderVm, sessionHostInfo.LogFolderId);
            await sessionHostRunner.CollectLogs(typeSpecificId, logFolder, sessionHostManager);
            await sessionHostRunner.TryDelete(typeSpecificId);
        }

        private void DownloadAndExtractAllAssets(SessionHostsStartInfo gameResourceDetails)
        {
            if (gameResourceDetails.AssetDetails?.Length > 0)
            {
                for (int i = 0; i < gameResourceDetails.AssetDetails.Length; i++)
                {
                    ExtractAndCopyAsset((i, gameResourceDetails.AssetDetails[i].LocalFilePath));
                }
            }
        }

        private void DownloadGameCertificates(SessionHostsStartInfo gameResourceDetails)
        {
            if (Globals.Settings.GameCertificateDetails?.Length > 0)
            {
                List<CertificateDetail> certs = new List<CertificateDetail>();
                foreach (GameCertificateDetails certUserDetails in Globals.Settings.GameCertificateDetails)
                {
                    if (!certUserDetails.Path.EndsWith(".pfx"))
                    {
                        Console.WriteLine($"Cert {certUserDetails.Path} is not a valid .pfx file. Skipping");
                        continue;
                    }
                    _systemOperations.FileCopy(
                        certUserDetails.Path,
                        Path.Combine(Globals.VmConfiguration.VmDirectories.CertificateRootFolderVm, Path.GetFileName(certUserDetails.Path)));

                    //we currently only support passwordless certificates
                    //passing a certificate with a password here will throw an exception because it requires a password parameter
                    X509Certificate2 cert = new X509Certificate2(certUserDetails.Path);
                    certs.Add(new CertificateDetail()
                    {
                        Name = certUserDetails.Name,
                        Thumbprint = cert.Thumbprint,
                        PfxContents = cert.RawData
                    });
                }
                if (Globals.Settings.RunContainer == false)
                {
                    // we're running in Process mode, so we'll let the user know that they must install the certs themselves in the cert store if needed
                    Console.WriteLine($@"Caution: Certificates {string.Join(",",
                        Globals.Settings.GameCertificateDetails.Select(x=>x.Name).ToList())}
                        were copied to game's certificate folder (accessible via GSDK) but not installed in machine's cert store. you may do that manually if you need to.");
                }
                gameResourceDetails.GameCertificates = certs.ToArray();
            }
        }

        /// <summary>
        /// Creates a port mapping list of the format:
        /// "Container Port", "VM Port", "Protocol"
        /// Ex: "81", "8001", "TCP"
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sessionHostInstance"></param>
        /// <returns></returns>
        private IList<PortMapping> GetPortMappings(SessionHostsStartInfo request, int sessionHostInstance)
        {
            if (request.PortMappingsList != null && request.PortMappingsList.Count > 0)
            {
                return request.PortMappingsList[sessionHostInstance];
            }

            return null;
        }

        private void ExtractAndCopyAsset((int assetNumber, string assetPath) assetDetail)
        {
            string assetFileName = _vmConfiguration.GetAssetDownloadFileName(assetDetail.assetPath);

            _basicAssetExtractor.ExtractAssets(assetFileName,
                _vmConfiguration.GetAssetExtractionFolderPathForSessionHost(0, assetDetail.assetNumber));
        }
    }
}
