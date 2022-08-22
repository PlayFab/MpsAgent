// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Dependencies
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using ApplicationInsights;
    using ApplicationInsights.DataContracts;
    using global::VmAgent.Core.Dependencies.Interfaces.Exceptions;
    using Microsoft.Azure.Gaming.VmAgent.Core.Dependencies.Interfaces;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class FileWriteWrapper : IFileWriteWrapper
    {
        private string _logFilePath;
        private StreamWriter _logWriter;

        public FileWriteWrapper()
        {
        }

        public void CreateFile(string logFilePath)
        {
            _logFilePath = logFilePath;
            _logWriter = new StreamWriter(File.OpenWrite(logFilePath));
        }

        public bool Close()
        {
            if (_logWriter != null)
            {
                _logWriter.Close();
                _logWriter = null;

                return true;
            }
            return false;
        }

        public void Write(string message)
        {
            if (_logWriter != null)
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

                _logWriter.WriteLine($"{currentDate}\t{message}");
                _logWriter.Flush();
            }
            else
            {
                new Exception($"StreamWriter is null or already closed. Cannot write a log to a file {_logFilePath}");
            }
        }

        public string GetProcessLogFilePath()
        {
            return _logFilePath;
        }
    }
}
