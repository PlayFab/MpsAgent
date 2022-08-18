﻿
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Gaming.VmAgent.Core;
using Microsoft.Azure.Gaming.VmAgent.Core.UnitTests;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VmAgent.Core.Dependencies.Interfaces.Exceptions;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class ProcessOutputLoggerTest
    {
        private MultiLogger _multiLogger;

        [TestInitialize]
        public void BeforeEachTest()
        {
            _multiLogger = new MultiLogger(NullLogger.Instance, new TelemetryClient(TelemetryConfiguration.CreateDefault()));
        }

        [TestMethod, TestCategory("BVT")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("\\abc")]
        public void InvalidLogFileNameFailed(string invalidFilePath)
        {
            ExceptionAssert.Throws<ProcessOuputLoggerCreationFailedException>(() => new ProcessOutputLogger(invalidFilePath, _multiLogger));
        }

        [TestMethod, TestCategory("BVT")]
        [DataRow("C:\\PF_Consolelog.txt")]
        public void ValidLogFileNameReturnFilePath(string validFilePath)
        {
            ProcessOutputLogger processLogger = new ProcessOutputLogger(validFilePath, _multiLogger);
            processLogger.GetProcessLogFilePath().Should().Equals(validFilePath);
        }

        [TestMethod, TestCategory("BVT")]
        public void CloseFileAfterCloseDoNotThrowException()
        {
            string fileFilePath = "C:\\PF_Consolelog.txt";
            ProcessOutputLogger processLogger = new ProcessOutputLogger(fileFilePath, _multiLogger);
            processLogger.Close();
            
            Action act = () => processLogger.Close();
            act.Should().NotThrow();
        }

        [TestMethod, TestCategory("BVT")]
        public void LogToFileAfterCloseFileDoNotThrowException()
        {
            string fileFilePath = "C:\\PF_Consolelog.txt";
            ProcessOutputLogger processLogger = new ProcessOutputLogger(fileFilePath, _multiLogger);
            processLogger.Close();
            
            Action act = () => processLogger.Log("ABCD");
            act.Should().NotThrow();
        }
    }
}
