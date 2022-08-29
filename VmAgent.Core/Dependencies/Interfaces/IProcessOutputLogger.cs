using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Azure.Gaming.VmAgent.Core.Dependencies.Interfaces
{
    public interface IProcessOutputLogger 
    {
        void Log(string message, string streamType);

        string GetProcessLogFilePath();

        void Close();

        void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine);

        void StdOutputHandler(object sendingProcess, DataReceivedEventArgs outLine);
    }
}
