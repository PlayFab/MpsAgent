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
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

        [TestMethod]
        [TestCategory("BVT")]
        public async Task DemuxDockerStream_SingleStdoutFrame_ExtractsContent()
        {
            string logLine = "BASH=/bin/bash\n";
            byte[] payload = Encoding.UTF8.GetBytes(logLine);
            byte[] frame = CreateDockerStreamFrame(1, payload); // 1 = stdout

            using var stream = new MemoryStream(frame);
            var output = new StringBuilder();

            await DockerContainerEngine.DemuxDockerStream(stream, content => output.Append(content));

            Assert.AreEqual(logLine, output.ToString());
        }

        [TestMethod]
        [TestCategory("BVT")]
        public async Task DemuxDockerStream_MultipleFrames_ExtractsAllContent()
        {
            string line1 = "BASH=/bin/bash\n";
            string line2 = "HOME=/root\n";
            byte[] frame1 = CreateDockerStreamFrame(1, Encoding.UTF8.GetBytes(line1));
            byte[] frame2 = CreateDockerStreamFrame(2, Encoding.UTF8.GetBytes(line2)); // 2 = stderr

            byte[] combined = new byte[frame1.Length + frame2.Length];
            Buffer.BlockCopy(frame1, 0, combined, 0, frame1.Length);
            Buffer.BlockCopy(frame2, 0, combined, frame1.Length, frame2.Length);

            using var stream = new MemoryStream(combined);
            var output = new StringBuilder();

            await DockerContainerEngine.DemuxDockerStream(stream, content => output.Append(content));

            Assert.AreEqual(line1 + line2, output.ToString());
        }

        [TestMethod]
        [TestCategory("BVT")]
        public async Task DemuxDockerStream_EmptyStream_ProducesNoOutput()
        {
            using var stream = new MemoryStream(Array.Empty<byte>());
            var output = new StringBuilder();

            await DockerContainerEngine.DemuxDockerStream(stream, content => output.Append(content));

            Assert.AreEqual(string.Empty, output.ToString());
        }

        [TestMethod]
        [TestCategory("BVT")]
        public async Task DemuxDockerStream_ZeroLengthPayload_SkipsFrame()
        {
            string logLine = "actual content\n";
            byte[] emptyFrame = CreateDockerStreamFrame(1, Array.Empty<byte>());
            byte[] contentFrame = CreateDockerStreamFrame(1, Encoding.UTF8.GetBytes(logLine));

            byte[] combined = new byte[emptyFrame.Length + contentFrame.Length];
            Buffer.BlockCopy(emptyFrame, 0, combined, 0, emptyFrame.Length);
            Buffer.BlockCopy(contentFrame, 0, combined, emptyFrame.Length, contentFrame.Length);

            using var stream = new MemoryStream(combined);
            var output = new StringBuilder();

            await DockerContainerEngine.DemuxDockerStream(stream, content => output.Append(content));

            Assert.AreEqual(logLine, output.ToString());
        }

        /// <summary>
        /// Creates a Docker multiplexed stream frame with the standard 8-byte header.
        /// </summary>
        private static byte[] CreateDockerStreamFrame(byte streamType, byte[] payload)
        {
            byte[] frame = new byte[8 + payload.Length];
            frame[0] = streamType;
            // bytes 1-3 are padding (zeros)
            // bytes 4-7 are payload size as big-endian uint32
            frame[4] = (byte)((payload.Length >> 24) & 0xFF);
            frame[5] = (byte)((payload.Length >> 16) & 0xFF);
            frame[6] = (byte)((payload.Length >> 8) & 0xFF);
            frame[7] = (byte)(payload.Length & 0xFF);
            Buffer.BlockCopy(payload, 0, frame, 8, payload.Length);
            return frame;
        }
    }
}