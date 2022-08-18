// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
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

    public class ProcessOutputLogger: IProcessOutputLogger
    {
        private string _logFilePath;
        private StreamWriter _logWriter;
        private MultiLogger _logger;

        public ProcessOutputLogger(string logFilePath, MultiLogger logger)
        {
            if (String.IsNullOrEmpty(logFilePath))
            {
                throw new ProcessOuputLoggerCreationFailedException($"LogFilePath cannot be null or empty.");
            }

            try
            {
                _logWriter = new StreamWriter(File.OpenWrite(logFilePath));
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception was thrown while creating a Process Log file under. {ex}");
                throw new ProcessOuputLoggerCreationFailedException($"Process Output Logger failed to create a file. {ex}.");
            }

            _logFilePath = logFilePath;
            _logger = logger;

            _logger.LogInformation($"Process Log file is created under {_logFilePath}");
        }

        public void Log(string message)
        {
            if (_logWriter == null)
            {
                _logger.LogInformation($"StreamWriter is null or not created yet.");
                return;
            }

            string currentDate = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

            _logWriter.WriteLine($"{currentDate}\t{message}");
            _logWriter.Flush();
        }

        public string GetProcessLogFilePath()
        {
            return _logFilePath;
        }

        public void Close()
        {
            _logger.LogInformation($"Closing a Process Log file... Target File Path : {GetProcessLogFilePath()}. ");

            if (_logWriter != null)
            {
                _logWriter.Close();
                _logWriter = null;
            }
            else
            {
                _logger.LogInformation($"Cannot close {_logFilePath}. StreamWriter is already null.");
            }
        }

        public void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Process p = sendingProcess as Process;
            if (p != null)
            {
                _logger.LogInformation($"ErrorOutputHandler was triggered - ProcessId: {p.Id}");
            }

            Log(outLine.Data);
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                _logger.LogException($"Exception was thrown while closing a file.", ex);
            }
        }

        public void StdOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Log(outLine.Data);
        }
    }
}
