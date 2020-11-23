// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent
{
    using AgentInterfaces;
    using ApplicationInsights;
    using ApplicationInsights.Extensibility;
    using Config;
    using Microsoft.Extensions.Logging;
    using VmAgent.Core;

    /// <summary>
    ///     Simple class to keep track of global values for the agent
    /// </summary>
    public static class Globals
    {
        public static MultiplayerSettings Settings;

        // Value updated from settings.json
        public static SessionConfig SessionConfig = null;

        public static VmConfiguration VmConfiguration { get; set; }

        public static ILogger Logger = LoggerFactory.Create(builder => { builder.AddConsole(); })
            .CreateLogger("PlayFabLocalMultiplayerAgent");

        public static MultiLogger MultiLogger =
            new MultiLogger(Logger, new TelemetryClient(TelemetryConfiguration.CreateDefault()));

        public static GameServerEnvironment GameServerEnvironment { get; set; }

        public static void AdaptFolderPathsForLinuxContainersOnWindows()
        {
            // running Linux containers with Docker for Windows requires some "weird" path mapping
            // in the sense that we want to map Linux paths on the container to Windows paths on the host
            // following method call makes sure of that
            VmDirectories vmd = VmConfiguration.VmDirectories;
            vmd.GameSharedContentFolderContainer = replacePathForLinuxContainersOnWindows(vmd.GameSharedContentFolderContainer);
            vmd.GameLogsRootFolderContainer = replacePathForLinuxContainersOnWindows(vmd.GameLogsRootFolderContainer);
            vmd.CertificateRootFolderContainer = replacePathForLinuxContainersOnWindows(vmd.CertificateRootFolderContainer);
            vmd.GsdkConfigRootFolderContainer = replacePathForLinuxContainersOnWindows(vmd.GsdkConfigRootFolderContainer);
            vmd.GsdkConfigFilePathContainer = replacePathForLinuxContainersOnWindows(vmd.GsdkConfigFilePathContainer);
        }

        private static string replacePathForLinuxContainersOnWindows(string windowspath)
        {
            return windowspath.Replace("\\", "/").Replace("C:/", "/data/");
        }
    }

    public enum GameServerEnvironment
    {
        Windows,
        Linux
    }
}
