// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.UnitTests
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using AgentInterfaces;
    using Core.Interfaces;
    using FluentAssertions;
    using global::VmAgent.Core.Interfaces;
    using LocalMultiplayerAgent.Config;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Azure.Gaming.VmAgent.Core;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using VisualStudio.TestTools.UnitTesting;

    public static class MultiplayerServerManagerTestUtil
    {
        internal static readonly Guid LegacyTestTitleId = new Guid("0b430f66-a5e4-4cca-8390-4324e84cc574");
        internal static readonly Guid TestTitleId = Guid.NewGuid();
        internal static readonly Guid TestBuildId = Guid.NewGuid();
        internal static readonly Guid TestVmID = Guid.NewGuid();
        internal static readonly string settingFilePath = "MultiplayerSettings.json";


        public static MultiplayerSettings CreateMultiplayerSetting(bool isLinuxGameServer = true)
        {
            MultiplayerSettings settings = new MultiplayerSettings();
            settings.TitleId = TestTitleId.ToString();
            settings.BuildId = TestBuildId;

            //MultiplayerSettings settings = JsonConvert.DeserializeObject<MultiplayerSettings>(File.ReadAllText(settingFilePath));
            if (isLinuxGameServer)
            {
                settings.RunContainer = true;
                settings.ContainerStartParameters = new ContainerStartParameters();
            }
            else
            {
                settings.RunContainer = false;
                settings.ProcessStartParameters = new ProcessStartParameters();
            }
            return settings;
        }

        //public static MultiplayerSettings CreateMultiplayerSettingForLinux()
        //{

        //}
        //public static ContainerStartParameters containerStartParamDetails()
        //{
        //    return new ContainerStartParameters { ImageDetails = new ContainerImageDetails() };
        //}
    }
}
