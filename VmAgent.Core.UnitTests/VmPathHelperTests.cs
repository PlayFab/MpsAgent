// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace VmAgent.Core.UnitTests
{
    using FluentAssertions;
    using Microsoft.Azure.Gaming.VmAgent.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VmPathHelperTests
    {
        /// <summary>
        /// Verifies that AdaptFolderPathsForLinuxContainersOnWindows correctly replaces
        /// backslashes with forward slashes and C:/ with /data/ for all container paths.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void AdaptFolderPaths_LinuxContainersOnWindows_TransformsAllContainerPaths()
        {
            string rootPath = @"D:\some\output\path";
            VmDirectories vmDirectories = new VmDirectories(rootPath);
            VmConfiguration vmConfiguration = new VmConfiguration(56001, "testVmId", vmDirectories, false);

            // Before adaptation, record original container paths (on Linux runner, these will use /data/ prefix already)
            // We explicitly set Windows-style paths to test the transformation
            vmConfiguration.VmDirectories.GameSharedContentFolderContainer = @"C:\GameSharedContent";
            vmConfiguration.VmDirectories.GameLogsRootFolderContainer = @"C:\GameLogs";
            vmConfiguration.VmDirectories.CertificateRootFolderContainer = @"C:\GameCertificates";
            vmConfiguration.VmDirectories.GsdkConfigRootFolderContainer = @"C:\Config";
            vmConfiguration.VmDirectories.GsdkConfigFilePathContainer = @"C:\Config\gsdkConfig.json";

            VmPathHelper.AdaptFolderPathsForLinuxContainersOnWindows(vmConfiguration);

            // Verify all paths are now Linux-style with /data/ prefix
            vmConfiguration.VmDirectories.GameSharedContentFolderContainer.Should().Be("/data/GameSharedContent");
            vmConfiguration.VmDirectories.GameLogsRootFolderContainer.Should().Be("/data/GameLogs");
            vmConfiguration.VmDirectories.CertificateRootFolderContainer.Should().Be("/data/GameCertificates");
            vmConfiguration.VmDirectories.GsdkConfigRootFolderContainer.Should().Be("/data/Config");
            vmConfiguration.VmDirectories.GsdkConfigFilePathContainer.Should().Be("/data/Config/gsdkConfig.json");
        }

        /// <summary>
        /// Verifies that AdaptFolderPathsForLinuxContainersOnWindows replaces backslashes with forward slashes.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void AdaptFolderPaths_LinuxContainersOnWindows_ReplacesBackslashes()
        {
            string rootPath = @"D:\output";
            VmDirectories vmDirectories = new VmDirectories(rootPath);
            VmConfiguration vmConfiguration = new VmConfiguration(56001, "testVmId", vmDirectories, false);

            vmConfiguration.VmDirectories.GameSharedContentFolderContainer = @"C:\some\nested\path";

            VmPathHelper.AdaptFolderPathsForLinuxContainersOnWindows(vmConfiguration);

            vmConfiguration.VmDirectories.GameSharedContentFolderContainer.Should().Be("/data/some/nested/path");
            vmConfiguration.VmDirectories.GameSharedContentFolderContainer.Should().NotContain("\\");
        }

        /// <summary>
        /// Verifies that VM-side paths are NOT modified by AdaptFolderPathsForLinuxContainersOnWindows.
        /// Only container paths should be adapted.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void AdaptFolderPaths_LinuxContainersOnWindows_DoesNotModifyVmPaths()
        {
            string rootPath = "root";
            VmDirectories vmDirectories = new VmDirectories(rootPath);
            VmConfiguration vmConfiguration = new VmConfiguration(56001, "testVmId", vmDirectories, false);

            // Record VM paths before adaptation
            string originalGameSharedContentFolderVm = vmConfiguration.VmDirectories.GameSharedContentFolderVm;
            string originalGameLogsRootFolderVm = vmConfiguration.VmDirectories.GameLogsRootFolderVm;
            string originalCertificateRootFolderVm = vmConfiguration.VmDirectories.CertificateRootFolderVm;
            string originalGsdkConfigRootFolderVm = vmConfiguration.VmDirectories.GsdkConfigRootFolderVm;

            VmPathHelper.AdaptFolderPathsForLinuxContainersOnWindows(vmConfiguration);

            // VM paths should remain unchanged
            vmConfiguration.VmDirectories.GameSharedContentFolderVm.Should().Be(originalGameSharedContentFolderVm);
            vmConfiguration.VmDirectories.GameLogsRootFolderVm.Should().Be(originalGameLogsRootFolderVm);
            vmConfiguration.VmDirectories.CertificateRootFolderVm.Should().Be(originalCertificateRootFolderVm);
            vmConfiguration.VmDirectories.GsdkConfigRootFolderVm.Should().Be(originalGsdkConfigRootFolderVm);
        }

        /// <summary>
        /// Verifies that for Windows processes (no Linux container adaptation), container paths
        /// retain their original platform-specific format.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void WindowsProcessPaths_NoAdaptation_ContainerPathsUnchanged()
        {
            string rootPath = "root";
            VmDirectories vmDirectories = new VmDirectories(rootPath);
            VmConfiguration vmConfiguration = new VmConfiguration(56001, "testVmId", vmDirectories, false);

            // For Windows processes, no path adaptation is called.
            // Container paths should remain as originally constructed by VmDirectories.
            string expectedSharedContent = vmConfiguration.VmDirectories.GameSharedContentFolderContainer;
            string expectedLogs = vmConfiguration.VmDirectories.GameLogsRootFolderContainer;
            string expectedCerts = vmConfiguration.VmDirectories.CertificateRootFolderContainer;
            string expectedConfig = vmConfiguration.VmDirectories.GsdkConfigRootFolderContainer;

            // These should match what VmDirectories constructed (using TempStorageRootContainer)
            expectedSharedContent.Should().Contain("GameSharedContent");
            expectedLogs.Should().Contain("GameLogs");
            expectedCerts.Should().Contain("GameCertificates");
            expectedConfig.Should().Contain("Config");
        }

        /// <summary>
        /// Verifies that for Windows containers (no Linux container adaptation), VM-side paths
        /// use the provided root path correctly.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void WindowsContainerPaths_NoAdaptation_VmPathsUseRootPath()
        {
            string rootPath = @"D:\myoutput";
            VmDirectories vmDirectories = new VmDirectories(rootPath);
            VmConfiguration vmConfiguration = new VmConfiguration(56001, "testVmId", vmDirectories, false);

            // VM paths should be based on the provided root path
            vmConfiguration.VmDirectories.TempStorageRootVm.Should().Be(rootPath);
            vmConfiguration.VmDirectories.GameSharedContentFolderVm.Should().Contain(rootPath);
            vmConfiguration.VmDirectories.GameLogsRootFolderVm.Should().Contain(rootPath);
            vmConfiguration.VmDirectories.CertificateRootFolderVm.Should().Contain(rootPath);
            vmConfiguration.VmDirectories.GsdkConfigRootFolderVm.Should().Contain(rootPath);
        }
    }
}
