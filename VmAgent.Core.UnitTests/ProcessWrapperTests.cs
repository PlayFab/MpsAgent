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
        /// so that it doesn't leak. After Kill, WaitForProcessExit should fall
        /// back to GetProcessById (which will throw because the process is gone).
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void Kill_RemovesProcessFromTrackedDictionary()
        {
            var wrapper = new ProcessWrapper();
            int pid = wrapper.Start(GetSleepProcessStartInfo());

            // Kill should remove from _trackedProcesses and dispose
            wrapper.Kill(pid);

            // WaitForProcessExit should now fall back to GetProcessById.
            // This throws ArgumentException if the process no longer exists,
            // or InvalidOperationException if the OS still has the PID but
            // the Process object wasn't the one that started it.
            Action act = () => wrapper.WaitForProcessExit(pid);
            act.Should().Throw<Exception>()
                .Which.Should().Match<Exception>(e =>
                    e is ArgumentException || e is InvalidOperationException);

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
            act.Should().Throw<Exception>()
                .Which.Should().Match<Exception>(e =>
                    e is ArgumentException || e is InvalidOperationException);

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
            try { Process.GetProcessById(pid).Kill(true); } catch { }
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
            act.Should().Throw<Exception>()
                .Which.Should().Match<Exception>(e =>
                    e is ArgumentException || e is InvalidOperationException);

            wrapper.Dispose();
        }

        private static ProcessStartInfo GetSleepProcessStartInfo()
        {
            // Cross-platform sleep: use dotnet to run a trivial inline program
            // that sleeps, or just use a long-running process
            if (OperatingSystem.IsWindows())
            {
                return new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c timeout /t 30 /nobreak >nul",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
            else
            {
                return new ProcessStartInfo
                {
                    FileName = "sleep",
                    Arguments = "30",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
        }

        private static ProcessStartInfo GetExitProcessStartInfo(int exitCode)
        {
            if (OperatingSystem.IsWindows())
            {
                return new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c exit {exitCode}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
            else
            {
                return new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = $"-c \"exit {exitCode}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
        }
    }
}
