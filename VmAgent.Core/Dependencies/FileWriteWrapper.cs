﻿// Copyright (c) Microsoft Corporation.
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
        private TextWriter _textWriter;

        public FileWriteWrapper()
        {
        }

        public void CreateFile(string logFilePath)
        {
            _logFilePath = logFilePath;
            _textWriter = TextWriter.Synchronized(new StreamWriter(File.OpenWrite(logFilePath)));
        }

        public bool Close()
        {
            if (_textWriter != null)
            {
                _textWriter.Close();
                _textWriter = null;

                return true;
            }
            return false;
        }

        public void Write(string message, string streamType = "stdout")
        {
            if (_textWriter != null)
            {
                string currentDate = DateTime.UtcNow.ToString("o");
                string logMessage = $"{{\"log\":{JsonConvert.ToString(message)}, \"stream\":\"{streamType}\", \"time\":\"{currentDate}\"}}";

                _textWriter.WriteLine(logMessage);
                _textWriter.Flush();
            }
            else
            {
                new InvalidOperationException($"StreamWriter is null or already closed. Cannot write a log to a file {_logFilePath}");
            }
        }

        public string GetProcessLogFilePath()
        {
            return _logFilePath;
        }
    }
}
