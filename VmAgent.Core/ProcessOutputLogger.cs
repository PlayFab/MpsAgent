// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    using System;
    using System.Diagnostics;
    using global::VmAgent.Core.Dependencies.Interfaces.Exceptions;
    using Microsoft.Azure.Gaming.VmAgent.Core.Dependencies;
    using Microsoft.Azure.Gaming.VmAgent.Core.Dependencies.Interfaces;

    public class ProcessOutputLogger: IProcessOutputLogger
    {
        private MultiLogger _logger;
        private IFileWriteWrapper _fileWriteWrapper;

        public ProcessOutputLogger(string logFilePath, MultiLogger logger) 
            :this (logFilePath, logger, new FileWriteWrapper()) 
        { 
        }

        public ProcessOutputLogger(string logFilePath, MultiLogger logger, IFileWriteWrapper fileWriteWrapper)
        {
            _logger = logger;
            _fileWriteWrapper = fileWriteWrapper;

            if (String.IsNullOrWhiteSpace(logFilePath))
            {
                throw new ProcessOuputLoggerCreationFailedException($"LogFilePath cannot be null or empty.");
            }

            try
            {
                _fileWriteWrapper.CreateFile(logFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogException($"Exception was thrown while creating a file. Closing a file {logFilePath}.", ex);
                Close();
            }
        }
        public void Log(string message)
        {
            try
            {
                _fileWriteWrapper.Write(message);
            }
            catch(Exception ex)
            {
                _logger.LogException($"Exception was thrown while writing a log. Closing a file {GetProcessLogFilePath()}.", ex);
                Close();
            }
        }

        public string GetProcessLogFilePath()
        {
            return _fileWriteWrapper.GetProcessLogFilePath();
        }

        public void Close()
        {
            _logger.LogInformation($"Closing a Process Log file... Target File Path : {GetProcessLogFilePath()}. ");

            try
            {
                if (_fileWriteWrapper.Close())
                {
                    _logger.LogInformation($"File is successfully closed.");
                }
                else
                {
                    _logger.LogInformation($"File is already closed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogException($"Exception was thrown while closing a file..", ex);
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
            Close();
        }

        public void StdOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Log(outLine.Data);
        }
    }
}
