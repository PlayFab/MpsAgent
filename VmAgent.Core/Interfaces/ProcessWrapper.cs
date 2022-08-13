// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class ProcessWrapper : IProcessWrapper
    {
        public void Kill(int id)
        {
            try
            {
                Process process = Process.GetProcessById(id);
                process.Kill(true);
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
            return Process.Start(startInfo).Id;
        }

        public int StartWithEventHandler(ProcessStartInfo startInfo, Action<object, DataReceivedEventArgs> StdOutputHandler, Action<object, DataReceivedEventArgs> ErrorOutputHandler)
        {
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.OutputDataReceived += new DataReceivedEventHandler(StdOutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(ErrorOutputHandler);
            process.EnableRaisingEvents = true;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process.Id;
        }

        public IEnumerable<int> List()
        {
            return Process.GetProcesses().Select(x => x.Id);
        }

        public void WaitForProcessExit(int id)
        {
            // TODO: this may need a bit more polish, it is currently only used by LocalMultiplayerAgent.
            Process process = Process.GetProcessById(id);
            process.WaitForExit();
        }
    }
}
