using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Azure.Gaming.VmAgent.Core.Dependencies.Interfaces
{
    public interface IFileWriteWrapper 
    {
        public void CreateFile(string logFilePath);
        public void Write(string message, string streamType = "stdout");
        public string GetProcessLogFilePath();
        bool Close();
    }
}
