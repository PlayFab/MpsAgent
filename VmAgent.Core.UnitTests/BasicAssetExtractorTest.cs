
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
using VmAgent.Core.Dependencies.Interfaces.Exceptions;

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

        private readonly string _targetFolder = @"/data/targetPath";

        [TestInitialize]
        public void BeforeEachTest()
        {
            _multiLogger = new MultiLogger(NullLogger.Instance, new TelemetryClient(TelemetryConfiguration.CreateDefault()));      
            _mockSystemOperations = new Mock<ISystemOperations>();
            _basicAssetExtractor = new BasicAssetExtractor(_mockSystemOperations.Object, _multiLogger);
        }

        [TestMethod, TestCategory("BVT")]
        [DataRow("testAsset.tar")]
        public void AssetExtractionFailedOnLinux(string asset)
        {
            SetUpMockOperationForTest(IsSetUpForWindows: false);

            _mockSystemOperations.Setup(x => x.RunProcessWithStdCapture(It.IsAny<Process>())).Returns((1, "stdOut", "stdErr"));

            ExceptionAssert.Throws<AssetExtractionFailedException>(() => _basicAssetExtractor.ExtractAssets(asset, _targetFolder));
        }

        [TestMethod, TestCategory("BVT")]
        [DataRow("")]
        [DataRow("testAsset")]
        [DataRow("testAsset.7z")]
        [DataRow("testAsset.rar")]
        [DataRow("testAsset.tar.gz")]
        [DataRow("testAsset.tar")]
        public void ExtractAssetsFailedNonSupportedExtensionsWindows(string archiveFileName)
        {
            SetUpMockOperationForTest(IsSetUpForWindows: true);

            ExceptionAssert.Throws<AssetExtractionFailedException>(() => _basicAssetExtractor.ExtractAssets(archiveFileName, _targetFolder));
        }

        [TestMethod, TestCategory("BVT")]
        [DataRow("")]
        [DataRow("testAsset")]
        [DataRow("testAsset.7z")]
        public void ExtractAssetsFailedNonSupportedExtensionsLinux(string archiveFileName)
        {
            SetUpMockOperationForTest(IsSetUpForWindows: false);

            ExceptionAssert.Throws<AssetExtractionFailedException>(() => _basicAssetExtractor.ExtractAssets(archiveFileName, _targetFolder));
        }

        [TestMethod, TestCategory("BVT")]
        [DataRow("testAsset.tar.gz")]
        [DataRow("testAsset.tar")]
        [DataRow("testAsset.zip")]
        public void VerifyProcessStartInfoForArchive(string archiveAssetFileName)
        {
            SetUpMockOperationForTest(IsSetUpForWindows: false);

            ProcessStartInfo testProcessInfo =
             Path.GetExtension(archiveAssetFileName).ToLowerInvariant() == Constants.ZipExtension
                  ? _basicAssetExtractor.GetProcessStartInfoForZip(archiveAssetFileName, _targetFolder)
                  : _basicAssetExtractor.GetProcessStartInfoForTarOrGZip(archiveAssetFileName, _targetFolder);

            ProcessStartInfo expectedProcessInfo = GetExpectedProcessInfoOnLinux(archiveAssetFileName, _targetFolder);

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

        public ProcessStartInfo GetExpectedProcessInfoOnLinux(string assetFileName, string targetFolder)
        {        
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

        public void SetUpMockOperationForTest(bool IsSetUpForWindows)
        {
            _mockSystemOperations.Setup(x => x.IsOSPlatform(OSPlatform.Windows)).Returns(IsSetUpForWindows);
        }

    }
}
