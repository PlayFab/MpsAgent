
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Gaming.VmAgent;
using Microsoft.Azure.Gaming.VmAgent.Core;
using Microsoft.Azure.Gaming.VmAgent.Core.Dependencies;
using Microsoft.Azure.Gaming.VmAgent.Core.Interfaces;
using Microsoft.Azure.Gaming.VmAgent.Core.UnitTests;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class BasicAssetExtractorTest
    {
        private BasicAssetExtractor _basicAssetExtractor;

        private MultiLogger _multiLogger;
        private Mock<ISystemOperations> _mockSystemOperations;

        private readonly string[] LinuxSupportedFileExtensions = 
            { Constants.ZipExtension, Constants.TarExtension, Constants.TarGZipExtension };

        private readonly string _TarAssetFileName = "testAsset.tar";
        private readonly string _TarGzAssetFileName = "testAsset.tar.gz";
        private readonly string _ZipAssetFileName = "testAsset.zip";
        private readonly string _targetFolder = @"/data/targetPath";

        [TestInitialize]
        public void BeforeEachTest()
        {
            _multiLogger = new MultiLogger(NullLogger.Instance, new TelemetryClient(TelemetryConfiguration.CreateDefault()));
            _mockSystemOperations = new Mock<ISystemOperations>();
            _basicAssetExtractor = new BasicAssetExtractor(_mockSystemOperations.Object, _multiLogger);
            
            _mockSystemOperations.Setup(x => x.IsOSPlatform(OSPlatform.Windows)).Returns(false);
        }


        [TestMethod, TestCategory("BVT")]
        public void AssetExtractionFailedOnLinux()
        {
            _mockSystemOperations.Setup(x => x.RunProcessWithStdCapture(It.IsAny<Process>())).Returns((1, "stdOut", "stdErr"));

            ExceptionAssert.Throws<Exception>(() => _basicAssetExtractor.ExtractAssets(_TarAssetFileName, _targetFolder));
        }

        [TestMethod, TestCategory("BVT")]
        public void VerifyProcessStartInfoForTar()
        {
            ProcessStartInfo testProcessInfo = 
                _basicAssetExtractor.GetProcessStartInfoForTarOrGZip(_TarAssetFileName, _targetFolder);

            ProcessStartInfo expectedProcessInfo = GetExpectedProcessInfoOnLinux(_TarAssetFileName, _targetFolder);

            VerifyValidProcessStartInfo(testProcessInfo, expectedProcessInfo);
        }

        [TestMethod, TestCategory("BVT")]
        public void VerifyProcessStartInfoForTarGZ()
        {
            ProcessStartInfo testProcessInfo =
                _basicAssetExtractor.GetProcessStartInfoForTarOrGZip(_TarGzAssetFileName, _targetFolder);

            ProcessStartInfo expectedProcessInfo = GetExpectedProcessInfoOnLinux(_TarGzAssetFileName, _targetFolder);

            VerifyValidProcessStartInfo(testProcessInfo, expectedProcessInfo);
        }

        [TestMethod, TestCategory("BVT")]
        public void VerifyProcessStartInfoForZipOnLinux()
        {
            ProcessStartInfo testProcessInfo =
                _basicAssetExtractor.GetProcessStartInfoForZip(_ZipAssetFileName, _targetFolder);

            ProcessStartInfo expectedProcessInfo = GetExpectedProcessInfoOnLinux(_ZipAssetFileName, _targetFolder);

            VerifyValidProcessStartInfo(testProcessInfo, expectedProcessInfo);
        }

        public void VerifyValidProcessStartInfo(ProcessStartInfo testProcessInfo, ProcessStartInfo expectedProcessInfo)
        {
            Assert.AreEqual(expectedProcessInfo.FileName, testProcessInfo.FileName);
            Assert.AreEqual(expectedProcessInfo.Arguments, testProcessInfo.Arguments);
            Assert.AreEqual(expectedProcessInfo.UseShellExecute, testProcessInfo.UseShellExecute);
            Assert.AreEqual(expectedProcessInfo.RedirectStandardError, testProcessInfo.RedirectStandardError);
            Assert.AreEqual(expectedProcessInfo.RedirectStandardOutput, testProcessInfo.RedirectStandardOutput);
            Assert.AreEqual(expectedProcessInfo.CreateNoWindow, testProcessInfo.CreateNoWindow);
        }

        public bool ValidateAssetFileName(string assetFileName)
        {
            return LinuxSupportedFileExtensions.Any(extension => assetFileName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase));
        }

        public ProcessStartInfo GetExpectedProcessInfoOnLinux(string assetFileName, string targetFolder)
        {
            Assert.IsTrue(ValidateAssetFileName(assetFileName));
            
            string fileExtension = Path.GetExtension(assetFileName).ToLowerInvariant();

            string argumentForExtraction = "";

            if (fileExtension == Constants.ZipExtension)
            {
                argumentForExtraction = $"-c \"unzip -oq {assetFileName} -d {targetFolder}\"";
            } else {
                string tarArguments = fileExtension == Constants.TarExtension ? "-xf" : "-xzf";
                argumentForExtraction = $"-c \"tar {tarArguments} {assetFileName} -C {targetFolder} --strip-components 1\"";
            }
            
            return new ProcessStartInfo()
            {
                FileName = "/bin/bash",
                Arguments = argumentForExtraction,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
        }
    }
}
