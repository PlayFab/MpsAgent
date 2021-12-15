using Microsoft.Azure.Gaming.VmAgent.Core.Dependencies.Interfaces;
using Microsoft.Azure.Gaming.VmAgent.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using VmAgent.Core.Dependencies.Interfaces.Exceptions;
using VmAgent.Core.Interfaces;

namespace Microsoft.Azure.Gaming.VmAgent.Core.Dependencies
{
    public class BasicAssetExtractor : IBasicAssetExtractor
    {
        private readonly ISystemOperations _systemOperations;
        private readonly MultiLogger _logger;

        private readonly string[] LinuxSupportedFileExtensions =
            { Constants.ZipExtension, Constants.TarExtension, Constants.TarGZipExtension };

        public BasicAssetExtractor(ISystemOperations systemOperations = null, MultiLogger logger = null)
        {
            _systemOperations = systemOperations;
            _logger = logger;
        }

        public void ExtractAssets(string assetFileName, string targetFolder)
        {
            string fileExtension = Path.GetExtension(assetFileName).ToLowerInvariant();

            // If the OS is windows use the native .NET zip extraction (we only support .zip for Windows).
            if (_systemOperations.IsOSPlatform(OSPlatform.Windows))
            {
                if (fileExtension != Constants.ZipExtension)
                {
                    throw new AssetExtractionFailedException($"Only Zip format is supported in Windows. Unable to extract {assetFileName}.");
                }

                if (Directory.Exists(targetFolder))
                {
                    Directory.Delete(targetFolder, true);
                }

                _systemOperations.ExtractToDirectory(assetFileName, targetFolder);
            }
            else
            {
                // If the OS is Linux, we need to extract files using Linux Command Line.
                if (!LinuxSupportedFileExtensions.Any(extension => assetFileName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new AssetExtractionFailedException($"Only Tar, Tar.gz and Zip formats are supported in Linux. Unable to extract {assetFileName}.");
                }

                _systemOperations.CreateDirectory(targetFolder);

                ProcessStartInfo processStartInfo = Path.GetExtension(assetFileName).ToLowerInvariant() == Constants.ZipExtension
                   ? GetProcessStartInfoForZip(assetFileName, targetFolder)
                   : GetProcessStartInfoForTarOrGZip(assetFileName, targetFolder);

                _logger.LogInformation($"Starting asset extraction with command arguments: {processStartInfo.Arguments}");

                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    (int exitCode, string stdOut, string stdErr) = _systemOperations.RunProcessWithStdCapture(process);

                    if (!string.IsNullOrEmpty(stdOut))
                    {
                        _logger.LogVerbose(stdOut);
                    }

                    if (!string.IsNullOrEmpty(stdErr))
                    {
                        _logger.LogError(stdErr);
                    }

                    if (exitCode != 0)
                    {
                        throw new AssetExtractionFailedException($"Asset extraction for file {assetFileName} failed. Errors: {stdErr ?? string.Empty}");
                    }

                    _systemOperations.SetUnixOwnerIfNeeded(targetFolder, true);
                }
            }
        }


        public ProcessStartInfo GetProcessStartInfoForZip(string assetFileName, string targetFolder)
        {
            return new ProcessStartInfo()
            {
                FileName = "/bin/bash",

                // o - overwrite, q - quiet, d - targetDirectory
                Arguments = $"-c \"unzip -oq {assetFileName} -d {targetFolder}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
        }

        public ProcessStartInfo GetProcessStartInfoForTarOrGZip(string assetFileName, string targetFolder)
        {
            // x - extract, z - decompress, f - filename (needs to be last argument)
            string tarArguments = Path.GetExtension(assetFileName).ToLowerInvariant() == Constants.TarExtension ? "-xf" : "-xzf";
            return new ProcessStartInfo()
            {
                FileName = "/bin/bash",

                // Tar extraction by default creates a new top level directory. Strip component allows to override that
                // and extract files directly in to the targetFolder.
                Arguments = $"-c \"tar {tarArguments} {assetFileName} -C {targetFolder} --strip-components 1\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
        }

    }
}
