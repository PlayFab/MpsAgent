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
    /// Simple class to keep track of global values for the agent
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
            new MultiLogger(Logger);

        public static GameServerEnvironment GameServerEnvironment { get; set; }
    }

    public enum GameServerEnvironment
    {
        Windows,
        Linux
    }
}
