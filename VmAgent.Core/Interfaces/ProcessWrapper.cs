// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class ProcessWrapper : IProcessWrapper, IDisposable
    {
        private readonly ConcurrentDictionary<int, Process> _trackedProcesses = new ConcurrentDictionary<int, Process>();

        public void Kill(int id)
        {
            try
            {
                // Use the tracked process reference when available to avoid PID reuse
                // issues and to ensure the dictionary entry is cleaned up.
                if (!_trackedProcesses.TryRemove(id, out Process process))
                {
                    process = Process.GetProcessById(id);
                }

                using (process)
                {
                    process.Kill(true);
                }
            }
            catch (ArgumentException)
            {
                // GetProcessById throws ArgumentException if the Process has already exited.
                // https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.Process/src/System/Diagnostics/Process.cs
            }
            catch (InvalidOperationException)
            {
                // Process.Kill throws InvalidOperationException if the Process has already exited.
                // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.kill?view=netcore-2.2
            }
        }

        public int Start(ProcessStartInfo startInfo)
        {
            Process process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Process.Start returned null for: " + startInfo.FileName);
            _trackedProcesses[process.Id] = process;
            return process.Id;
        }

        public int StartWithEventHandler(ProcessStartInfo startInfo, Action<object, DataReceivedEventArgs> StdOutputHandler, Action<object, DataReceivedEventArgs> ErrorOutputHandler, Action<object, EventArgs> ProcessExitedHandler)
        {
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.OutputDataReceived += new DataReceivedEventHandler(StdOutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(ErrorOutputHandler);
            process.Exited += new EventHandler(ProcessExitedHandler);
            process.EnableRaisingEvents = true;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            _trackedProcesses[process.Id] = process;
            return process.Id;
        }

        public IEnumerable<int> List()
        {
            return Process.GetProcesses().Select(x => x.Id);
        }

        public int WaitForProcessExit(int id)
        {
            if (!_trackedProcesses.TryRemove(id, out Process process))
            {
                throw new InvalidOperationException(
                    $"Process {id} is not tracked. All processes should be started through this wrapper.");
            }

            using (process)
            {
                try
                {
                    process.WaitForExit();
                    return process.ExitCode;
                }
                finally
                {
                    try { process.CancelOutputRead(); }
                    catch (InvalidOperationException) { /* expected when output was not redirected */ }

                    try { process.CancelErrorRead(); }
                    catch (InvalidOperationException) { /* expected when error was not redirected */ }
                }
            }
        }

        public void Dispose()
        {
            foreach (var kvp in _trackedProcesses)
            {
                if (_trackedProcesses.TryRemove(kvp.Key, out Process process))
                {
                    process.Dispose();
                }
            }
        }
    }
}
