// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using AgentInterfaces;
    using AspNetCore.Hosting;
    using Config;
    using Newtonsoft.Json;
    using VmAgent.Core;
    using VmAgent.Core.Interfaces;
    using Microsoft.Azure.Gaming.LocalMultiplayerAgent.MPSDeploymentTool;

    public class Program
    {
        public static async Task Main(string[] args)
        {

            string[] salutations =
            {
                "Have a nice day!",
                "Thank you for using PlayFab Multiplayer Servers",
                "Check out our docs at aka.ms/playfabdocs!",
                "Have a question? Check our community at community.playfab.com"
            };
            Console.WriteLine(salutations[new Random().Next(salutations.Length)]);

            string debuggingUrl = "https://github.com/PlayFab/gsdkSamples/blob/master/Debugging.md";
            Console.WriteLine($"Check this page for debugging tips: {debuggingUrl}");

            // lcow stands for Linux Containers On Windows => https://docs.microsoft.com/en-us/virtualization/windowscontainers/deploy-containers/linux-containers
            Globals.GameServerEnvironment = args.Contains("-lcow") && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? GameServerEnvironment.Linux : GameServerEnvironment.Windows; // LocalMultiplayerAgent is running only on Windows for the time being
            Globals.CreateDeployment = args.Contains("-deploy");

            MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(File.ReadAllText("MultiplayerSettings.json"));

            if (!Globals.CreateDeployment)
            {
                settings.SetDefaultsIfNotSpecified();
            }

            MultiplayerSettingsValidator validator = new MultiplayerSettingsValidator(settings);

            if (!validator.IsValid())
            {
                Console.WriteLine("The specified settings are invalid. Please correct them and re-run the agent.");
                Environment.Exit(1);
            }

            if (Globals.CreateDeployment)
            {
                DeploymentScript deploymentScript = new DeploymentScript(settings);
                await deploymentScript.RunScriptAsync();
                return;
            }

            string vmId =
                $"xcloudwusu4uyz5daouzl:{settings.Region}:{Guid.NewGuid()}:tvmps_{Guid.NewGuid():N}{Guid.NewGuid():N}_d";

            Console.WriteLine($"TitleId: {settings.TitleId}");
            Console.WriteLine($"BuildId: {settings.BuildId}");
            Console.WriteLine($"VmId: {vmId}");

            Globals.Settings = settings;
            string rootOutputFolder = Path.Combine(settings.OutputFolder, "PlayFabVmAgentOutput", DateTime.Now.ToString("s").Replace(':', '-'));
            Console.WriteLine($"Root output folder: {rootOutputFolder}");

            VmDirectories vmDirectories = new VmDirectories(rootOutputFolder);

            Globals.VmConfiguration = new VmConfiguration(settings.AgentListeningPort, vmId, vmDirectories, false);
            if (Globals.GameServerEnvironment == GameServerEnvironment.Linux && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                VmPathHelper.AdaptFolderPathsForLinuxContainersOnWindows(Globals.VmConfiguration);  // Linux Containers on Windows requires special folder mapping
            }

            Directory.CreateDirectory(rootOutputFolder);
            Directory.CreateDirectory(vmDirectories.GameLogsRootFolderVm);
            Directory.CreateDirectory(Globals.VmConfiguration.VmDirectories.CertificateRootFolderVm);
            IWebHost host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{settings.AgentListeningPort}")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            await host.StartAsync();

            Console.WriteLine($"Local Multiplayer Agent is listening on port {settings.AgentListeningPort}");

            Globals.SessionConfig = settings.SessionConfig ?? new SessionConfig() { SessionId = Guid.NewGuid() };
            Console.WriteLine($"{string.Join(", ", Globals.SessionConfig.InitialPlayers)}");
            await new MultiplayerServerManager(SystemOperations.Default, Globals.VmConfiguration, Globals.MultiLogger, SessionHostRunnerFactory.Instance)
                .CreateAndStartContainerWaitForExit(settings.ToSessionHostsStartInfo());

            await host.StopAsync();
        }
    }
}
