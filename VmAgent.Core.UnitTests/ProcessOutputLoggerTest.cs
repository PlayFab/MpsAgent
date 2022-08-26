
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Gaming.VmAgent.Core;
using Microsoft.Azure.Gaming.VmAgent.Core.Dependencies.Interfaces;
using Microsoft.Azure.Gaming.VmAgent.Core.UnitTests;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using VmAgent.Core.Dependencies.Interfaces.Exceptions;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class ProcessOutputLoggerTest
    {
        private string _validPath = "C:\\PF_Consolelog.txt";
        private MultiLogger _multiLogger;
        private Mock<IFileWriteWrapper> _fileWriteWrapper;
        private ProcessOutputLogger _processLogger;

        [TestInitialize]
        public void BeforeEachTest()
        {
            _multiLogger = new MultiLogger(NullLogger.Instance, new TelemetryClient(TelemetryConfiguration.CreateDefault()));
            _fileWriteWrapper = new Mock<IFileWriteWrapper>();

            Mock<ProcessOutputLogger> mockProcessLogger = new Mock<ProcessOutputLogger>(
               _validPath,
               _multiLogger,
               _fileWriteWrapper.Object)
            {
                CallBase = true
            };

            _processLogger = mockProcessLogger.Object;
        }

        [TestMethod, TestCategory("BVT")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
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
        public void FileWriteFailDoNotThrowException()
        {
            _fileWriteWrapper.Setup(x => x.Write(It.IsAny<string>(), It.IsAny<string>())).Throws(new InvalidOperationException());

            Action act = () => _processLogger.Log("test123");
            act.Should().NotThrow();
        }

        [TestMethod, TestCategory("BVT")]
        public void FileCloseFailDoNotThrowException()
        {
            _fileWriteWrapper.Setup(x => x.Close()).Throws(new Exception());
            
            Action act = () => _processLogger.Close();
            act.Should().NotThrow();
        }

    }
}
