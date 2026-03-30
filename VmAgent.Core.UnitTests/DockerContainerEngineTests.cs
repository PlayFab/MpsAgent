// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Azure.Gaming.VmAgent.ContainerEngines;
using Microsoft.Azure.Gaming.VmAgent.Core;
using Microsoft.Azure.Gaming.VmAgent.Core.Interfaces;
using Microsoft.Azure.Gaming.VmAgent.Model;
using Microsoft.Azure.Gaming.AgentInterfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class DockerContainerEngineTests
    {
        private Mock<ISystemOperations> _mockSystemOperations;
        private DockerContainerEngine _dockerContainerEngine;

        [TestInitialize]
        public void BeforeEachTest()
        {
            _mockSystemOperations = new Mock<ISystemOperations>();
            var logger = new MultiLogger(NullLogger.Instance);
            var vmConfiguration = new VmConfiguration(56001, "testVmId", new VmDirectories("root"), true);
            _dockerContainerEngine = new DockerContainerEngine(vmConfiguration, logger, _mockSystemOperations.Object);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestGetImageNameWithTag()
        {
            var imageDetails = new ContainerImageDetails
            {
                ImageName = "name",
                ImageTag = "tag",
                ImageDigest = null,
                Registry = null,
            };
            Assert.AreEqual("name:tag", DockerContainerEngine.GetImageNameAndTagFromContainerImageDetails(imageDetails));

            imageDetails.Registry = "registry";
            Assert.AreEqual("registry/name:tag", DockerContainerEngine.GetImageNameAndTagFromContainerImageDetails(imageDetails));
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetStartGameCmd_LinuxContainersOnWindows_ShouldNotAddCmdPrefix()
        {
            _mockSystemOperations.Setup(x => x.IsOSPlatform(OSPlatform.Windows)).Returns(true);

            var request = new SessionHostsStartInfo { StartGameCommand = "sleep 100" };

            IList<string> result = _dockerContainerEngine.GetStartGameCmd(request, isLinuxContainersOnWindows: true);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("sleep 100", result[0]);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetStartGameCmd_WindowsContainersOnWindows_ShouldAddCmdPrefix()
        {
            _mockSystemOperations.Setup(x => x.IsOSPlatform(OSPlatform.Windows)).Returns(true);

            var request = new SessionHostsStartInfo { StartGameCommand = "myGame.exe" };

            IList<string> result = _dockerContainerEngine.GetStartGameCmd(request, isLinuxContainersOnWindows: false);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("cmd /c myGame.exe", result[0]);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetStartGameCmd_LinuxHost_ShouldNotAddCmdPrefix()
        {
            _mockSystemOperations.Setup(x => x.IsOSPlatform(OSPlatform.Windows)).Returns(false);

            var request = new SessionHostsStartInfo { StartGameCommand = "sleep 100" };

            IList<string> result = _dockerContainerEngine.GetStartGameCmd(request, isLinuxContainersOnWindows: false);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("sleep 100", result[0]);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetStartGameCmd_EmptyCommand_ReturnsNull()
        {
            var request = new SessionHostsStartInfo { StartGameCommand = "" };

            IList<string> result = _dockerContainerEngine.GetStartGameCmd(request, isLinuxContainersOnWindows: false);

            Assert.IsNull(result);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetGameWorkingDir_LinuxContainersOnWindows_ShouldReturnNull()
        {
            _mockSystemOperations.Setup(x => x.IsOSPlatform(OSPlatform.Windows)).Returns(true);

            var request = new SessionHostsStartInfo
            {
                StartGameCommand = "sleep 100",
                GameWorkingDirectory = "/app"
            };

            string result = _dockerContainerEngine.GetGameWorkingDir(request, isLinuxContainersOnWindows: true);

            Assert.IsNull(result);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetGameWorkingDir_WindowsContainersOnWindows_ShouldReturnWorkingDir()
        {
            _mockSystemOperations.Setup(x => x.IsOSPlatform(OSPlatform.Windows)).Returns(true);

            var request = new SessionHostsStartInfo
            {
                GameWorkingDirectory = @"C:\GameServer"
            };

            string result = _dockerContainerEngine.GetGameWorkingDir(request, isLinuxContainersOnWindows: false);

            Assert.AreEqual(@"C:\GameServer", result);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetGameWorkingDir_WindowsContainersOnWindows_DerivedFromStartGameCommand()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Path.GetDirectoryName uses the host OS path separator,
                // so this test is only valid on Windows.
                Assert.Inconclusive("This test requires Windows to validate Windows path handling.");
            }

            _mockSystemOperations.Setup(x => x.IsOSPlatform(OSPlatform.Windows)).Returns(true);

            var request = new SessionHostsStartInfo
            {
                StartGameCommand = @"C:\GameServer\myGame.exe -arg1"
            };

            string result = _dockerContainerEngine.GetGameWorkingDir(request, isLinuxContainersOnWindows: false);

            Assert.AreEqual(@"C:\GameServer", result);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetGameWorkingDir_LinuxHost_ShouldReturnNull()
        {
            _mockSystemOperations.Setup(x => x.IsOSPlatform(OSPlatform.Windows)).Returns(false);

            var request = new SessionHostsStartInfo
            {
                StartGameCommand = "/app/gameserver",
                GameWorkingDirectory = "/app"
            };

            string result = _dockerContainerEngine.GetGameWorkingDir(request, isLinuxContainersOnWindows: false);

            Assert.IsNull(result);
        }
    }
}