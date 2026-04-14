// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace VmAgent.Core.UnitTests
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Gaming.VmAgent.Core.Interfaces;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using FluentAssertions;

    [TestClass]
    public class ProcessWrapperTests
    {
        /// <summary>
        /// Verifies that Kill() removes the Process from the tracked dictionary
        /// so that it doesn't leak. After Kill, WaitForProcessExit should throw
        /// InvalidOperationException because the process is no longer tracked.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void Kill_RemovesProcessFromTrackedDictionary()
        {
            var wrapper = new ProcessWrapper();
            int pid = wrapper.Start(GetSleepProcessStartInfo());

            wrapper.Kill(pid);

            // WaitForProcessExit should throw because the process was removed from tracking by Kill
            Action act = () => wrapper.WaitForProcessExit(pid);
            act.Should().Throw<InvalidOperationException>();

            wrapper.Dispose();
        }

        /// <summary>
        /// Verifies that WaitForProcessExit returns the correct exit code
        /// and removes the tracked process.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void WaitForProcessExit_ReturnsExitCodeAndCleansUp()
        {
            var wrapper = new ProcessWrapper();

            // Start a process that exits with code 0
            var startInfo = GetExitProcessStartInfo(exitCode: 0);
            int pid = wrapper.Start(startInfo);

            int exitCode = wrapper.WaitForProcessExit(pid);
            exitCode.Should().Be(0);

            // Calling again should throw since it was removed from tracking
            Action act = () => wrapper.WaitForProcessExit(pid);
            act.Should().Throw<InvalidOperationException>();

            wrapper.Dispose();
        }

        /// <summary>
        /// Verifies that exit code is captured correctly even for non-zero codes.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void WaitForProcessExit_CapturesNonZeroExitCode()
        {
            var wrapper = new ProcessWrapper();

            var startInfo = GetExitProcessStartInfo(exitCode: 42);
            int pid = wrapper.Start(startInfo);

            int exitCode = wrapper.WaitForProcessExit(pid);
            exitCode.Should().Be(42);

            wrapper.Dispose();
        }

        /// <summary>
        /// Verifies that Kill handles an already-exited process gracefully
        /// (no exceptions thrown) and still cleans up the dictionary entry.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void Kill_AlreadyExitedProcess_DoesNotThrow()
        {
            var wrapper = new ProcessWrapper();

            var startInfo = GetExitProcessStartInfo(exitCode: 0);
            int pid = wrapper.Start(startInfo);

            // Wait for the process to exit on its own
            Process.GetProcessById(pid).WaitForExit(5000);
            Thread.Sleep(100); // small buffer

            // Kill should not throw even though the process already exited
            Action act = () => wrapper.Kill(pid);
            act.Should().NotThrow();

            wrapper.Dispose();
        }

        /// <summary>
        /// Verifies that Dispose cleans up any remaining tracked processes.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void Dispose_CleansUpRemainingTrackedProcesses()
        {
            var wrapper = new ProcessWrapper();
            int pid = wrapper.Start(GetSleepProcessStartInfo());

            // Dispose without Kill or WaitForProcessExit — should not leak
            wrapper.Dispose();

            // Clean up the actual OS process
            try { Process.GetProcessById(pid).Kill(true); }
            catch (ArgumentException) { /* process already exited */ }
            catch (InvalidOperationException) { /* process already exited */ }
        }

        /// <summary>
        /// Verifies that StartWithEventHandler tracks the process and Kill
        /// cleans it up properly.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void StartWithEventHandler_KillCleansUpTrackedProcess()
        {
            var wrapper = new ProcessWrapper();
            int pid = wrapper.StartWithEventHandler(
                GetSleepProcessStartInfo(),
                (sender, args) => { },
                (sender, args) => { },
                (sender, args) => { });

            wrapper.Kill(pid);

            // Process should be removed from tracking
            Action act = () => wrapper.WaitForProcessExit(pid);
            act.Should().Throw<InvalidOperationException>();

            wrapper.Dispose();
        }

        private static ProcessStartInfo GetSleepProcessStartInfo() =>
            OperatingSystem.IsWindows()
                ? new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c timeout /t 30 /nobreak >nul",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
                : new ProcessStartInfo
                {
                    FileName = "sleep",
                    Arguments = "30",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

        private static ProcessStartInfo GetExitProcessStartInfo(int exitCode) =>
            OperatingSystem.IsWindows()
                ? new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c exit {exitCode}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
                : new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = $"-c \"exit {exitCode}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
    }
}
