using Microsoft.Azure.Gaming.VmAgent.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Azure.Gaming.VmAgent.Core.Dependencies
{
    public class BasicAssetExtractor
    {
        public static BasicAssetExtractor Instance = new BasicAssetExtractor();

        private readonly ISystemOperations _systemOperations;
        private readonly MultiLogger _logger;
        private string _zipExtension;
        private string _tarExtension;

        public BasicAssetExtractor(ISystemOperations systemOperations = null, MultiLogger logger = null, string zipExtension = null, string tarExtension = null)
        { 
            _systemOperations = systemOperations;
            _logger = logger;
            _zipExtension = zipExtension;
            _tarExtension = tarExtension;
        }

        public void ExtractAssets(string assetFileName, string targetFolder)
        {
            // If the OS is windows use the native .NET zip extraction (we only support .zip for Windows).
            if (_systemOperations.IsOSPlatform(OSPlatform.Windows))
            {
                if (Directory.Exists(targetFolder))
                {
                    Directory.Delete(targetFolder, true);
                }

                _systemOperations.ExtractToDirectory(assetFileName, targetFolder);
            }
            else
            {
                _systemOperations.CreateDirectory(targetFolder);

                ProcessStartInfo processStartInfo = Path.GetExtension(assetFileName).ToLowerInvariant() == _zipExtension
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
                        throw new Exception($"Asset extraction for file {assetFileName} failed. Errors: {stdErr ?? string.Empty}");
                    }

                    _systemOperations.SetUnixOwnerIfNeeded(targetFolder, true);
                }
            }
        }

        private ProcessStartInfo GetProcessStartInfoForZip(string assetFileName, string targetFolder)
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

        private ProcessStartInfo GetProcessStartInfoForTarOrGZip(string assetFileName, string targetFolder)
        {
            // x - extract, z - decompress, f - filename (needs to be last argument)
            string tarArguments = Path.GetExtension(assetFileName).ToLowerInvariant() == _tarExtension ? "-xf" : "-xzf";
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
