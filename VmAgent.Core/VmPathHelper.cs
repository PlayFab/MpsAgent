// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    using AgentInterfaces;
    using ApplicationInsights;
    using ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;
    using VmAgent.Core;

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

        public static void AdaptFolderPathsForLinuxContainersOnMacOS(VmConfiguration VmConfiguration)
        {
            // running Linux containers with Docker for Mac requires path mapping
            // Container paths already use Linux-style /data/ prefix which is correct for Linux containers
            // No additional adaptation is needed since MacOS host paths use forward slashes natively
            // and VmDirectories already sets container paths correctly for non-Windows platforms
        }

        private static string replacePathForLinuxContainersOnWindows(string windowspath)
        {
            return windowspath.Replace("\\", "/").Replace("C:/", "/data/");
        }
    }
}
