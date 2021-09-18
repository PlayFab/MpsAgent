// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    using AgentInterfaces;
    using ApplicationInsights;
    using ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;
    using VmAgent.Core;

    /// <summary>
    ///     Simple class to keep track of global values for the agent
    /// </summary>
    public static class VmPathHelper
    {
        public static void AdaptFolderPathsForLinuxContainersOnWindows(VmConfiguration VmConfiguration)
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
}
