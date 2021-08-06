// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using Mono.Unix;
    using Mono.Unix.Native;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using OpenFlags = System.Security.Cryptography.X509Certificates.OpenFlags;

    public class SystemOperations : ISystemOperations
    {
        public static SystemOperations Default = new SystemOperations();
        private VmConfiguration _vmConfiguration;
        private MultiLogger _logger;

        public SystemOperations(VmConfiguration vmConfiguration = null, MultiLogger logger = null)
        {
            _vmConfiguration = vmConfiguration;
            _logger = logger;
        }

        public DateTime UtcNow => DateTime.UtcNow;
        public void ExtractToDirectory(string sourcePath, string targetPath)
        {
            // Clean up the directory if it's already there before extracting,
            // otherwise we'll get an exception
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath);
            }
            DirectoryInfo dirInfo = new DirectoryInfo(targetPath);
            //Create parent directories to ensure correct ownership
            CreateDirectory(dirInfo.Parent.FullName);

            ZipFile.ExtractToDirectory(sourcePath, targetPath);
            SetUnixOwnerIfNeeded(targetPath, true);

        }

        public void CreateZipFile(string sourceDirectory, string destinationFilename)
        {
            ZipFile.CreateFromDirectory(sourceDirectory, destinationFilename);
            SetUnixOwnerIfNeeded(destinationFilename);
        }

        public Task Delay(int milliseconds)
        {
            return Task.Delay(milliseconds);
        }
        
        public Task Delay(int milliseconds, CancellationToken token)
        {
            return Task.Delay(milliseconds, token);
        }

        public Task Delay(TimeSpan timeSpan)
        {
            return Task.Delay(timeSpan);
        }

        public Guid NewGuid()
        {
            return Guid.NewGuid();
        }

        public void DeleteDirectoryIfExists(string pathToDirectory)
        {
            if (Directory.Exists(pathToDirectory))
            {
                Directory.Delete(pathToDirectory, true);
            }
        }

        public bool IsDirectoryEmpty(string pathToDirectory)
        {
            return !Directory.EnumerateFileSystemEntries(pathToDirectory).Any();
        }

        public Stream OpenFileForRead(string filePath)
        {
            return File.OpenRead(filePath);
        }

        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        public void CreateDirectory(string fullPath)
        {
            //Create all missing directories in the file tree explicitly to ensure proper ownership
            DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
            if (!directoryInfo.Parent.Exists)
            {
                CreateDirectory(directoryInfo.Parent.FullName);
            }
            // Do this check to prevent owning /mnt by accident
            if(!directoryInfo.Exists)
            {
                directoryInfo.Create();
                SetUnixOwnerIfNeeded(fullPath);
            }
        }

        // From MSDN: https://msdn.microsoft.com/en-us/library/system.io.directoryinfo.aspx
        public void CopyDirectoryContents(DirectoryInfo source, DirectoryInfo target)
        {
            if (string.Equals(source.FullName, target.FullName, StringComparison.Ordinal))
            {
                return;
            }

            // Note, if the directory already exists, it doesn't try to recreate it.
           CreateDirectory(target.FullName);

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
                SetUnixOwnerIfNeeded(Path.Combine(target.ToString(), fi.Name));
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                this.CopyDirectoryContents(diSourceSubDir, nextTargetSubDir);
            }
        }

        public void FileAppendAllText(string path, string contents)
        {
            FileInfo fileInfo = new FileInfo(path);
            //Create parent directories to ensure correct ownership
            CreateDirectory(fileInfo.DirectoryName);

            File.AppendAllText(path, contents);
            SetUnixOwnerIfNeeded(path);
        }
        
        public void FileWriteAllText(string path, string contents)
        {
            FileInfo fileInfo = new FileInfo(path);
            //Create parent directories to ensure correct ownership
            CreateDirectory(fileInfo.DirectoryName);

            File.WriteAllText(path, contents);
            SetUnixOwnerIfNeeded(path);
        }

        public void FileCopy(string sourceFilePath, string destinationFilePath)
        {
            FileInfo fileInfo = new FileInfo(destinationFilePath);
            //Create parent directories to ensure correct ownership
            CreateDirectory(fileInfo.DirectoryName);

            File.Copy(sourceFilePath, destinationFilePath);
            SetUnixOwnerIfNeeded(destinationFilePath);
        }

        public void FileMoveWithOverwrite(string sourceFilePath, string destinationFilePath)
        {
            FileInfo fileInfo = new FileInfo(destinationFilePath);
            //Create parent directories to ensure correct ownership
            CreateDirectory(fileInfo.DirectoryName);

            File.Move(sourceFilePath, destinationFilePath, true);
            SetUnixOwnerIfNeeded(destinationFilePath);
        }

        public async Task FileWriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken = default(CancellationToken))
        {
            FileInfo fileInfo = new FileInfo(path);
            //Create parent directories to ensure correct ownership
            CreateDirectory(fileInfo.DirectoryName);

            await File.WriteAllBytesAsync(path, content, cancellationToken);
            SetUnixOwnerIfNeeded(path);
        }

        public async Task FileWriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default(CancellationToken))
        {
            FileInfo fileInfo = new FileInfo(path);
            //Create parent directories to ensure correct ownership
            CreateDirectory(fileInfo.DirectoryName);

            await File.WriteAllTextAsync(path, content, cancellationToken);
            SetUnixOwnerIfNeeded(path);
        }

        public long FileInfoLength(string path)
        {
            return new FileInfo(path).Length;
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string FileReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void ExitProcess(int exitCode)
        {
            Environment.Exit(exitCode);
        }

        public (int exitCode, string stdOut, string stdErr) RunProcessWithStdCapture(Process process)
        {
            // Asynchronously read the output and errors to avoid deadlocks.
            // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.redirectstandardoutput?view=netcore-3.0
            string stdError = string.Empty;
            string stdOut = string.Empty;
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                stdError += e.Data;
            });

            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                stdOut += e.Data;
            });

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();
            return (process.ExitCode, stdOut, stdError);
        }

        public int RunProcessWithoutStdCapture(Process process)
        {
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }

        public bool IsOSPlatform(OSPlatform platform)
        {
            return RuntimeInformation.IsOSPlatform(platform);
        }

        public void InstallCertificate(byte[] certificateContent)
        {
            using (var cert = new X509Certificate2(certificateContent, (string)null,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable))
            {
                using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.ReadWrite))
                {
                    store.Add(cert);
                    store.Close();
                }

                using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadWrite))
                {
                    store.Add(cert);
                    store.Close();
                }
            }
        }

        public void UninstallCertificate(byte[] certificateContent)
        {
            // The using around cert creation is necessary to ensure deterministic and timely dispose of the private keys (at the end of the execution of this block).
            // We have seen that the private keys suddenly get deleted for many process-based game servers due to the following sequence of events (when using is not used).
            // 1) We call uninstall (this method) on the cert (to remove residues from any previous VM Assignments).
            // 2) We call install the cert.
            // 3) The game server or another executable tries to use the cert.
            // At some point between 2 and 3, the dispose of the cert is executed from 1) which actually ends up deleting the private keys from the key store.
            // The certificate then becomes unusable / throws an error that the private key is not found.
            // This affects only process based servers since containers have the cert installed within the container and there is no cleanup required after a container tear down.
            using (var cert = new X509Certificate2(certificateContent))
            {
                using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.ReadWrite))
                {
                    store.Remove(cert);
                    store.Close();
                }

                using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadWrite))
                {
                    store.Remove(cert);
                    store.Close();
                }
            }
        }

        public string GetHostName()
        {
            return Dns.GetHostName();
        }

        public IPHostEntry GetHostEntry(string hostName)
        {
            return Dns.GetHostEntry(hostName);
        }

        public void SetUnixFilePermissions(string filePath, int permissions)
        {
            var unixFileInfo = new UnixFileInfo(filePath);
            unixFileInfo.FileAccessPermissions = (FileAccessPermissions)permissions;
        }

        private void SetUnixOwner(string filePath, string owner)
        {
            var passwd = Syscall.getpwnam(owner);
            if(passwd == null)
            {
                throw new ArgumentException($"The specified user {owner} could not be retrieved");
            }
            var result = Syscall.chown(filePath, passwd.pw_uid, passwd.pw_gid);
            if(result != 0)
            {
                throw new ArgumentException($"Failed to give ownership of {filePath} to {owner}");
            }
        }

        public void SetUnixOwnerIfNeeded(string path, bool applyToAllContents = false)
        {

            if (_vmConfiguration != null && _vmConfiguration.RunContainersInUsermode)
            {
                _logger.LogInformation($"Setting unix owner for {path}");
                SetUnixOwner(path, "glados");

                FileAttributes attr = File.GetAttributes(path);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory && applyToAllContents)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(path);
                    IEnumerable<FileInfo> files = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories);
                    IEnumerable<DirectoryInfo> subDirectories = dirInfo.EnumerateDirectories("*", SearchOption.AllDirectories);

                    foreach (FileInfo file in files)
                    {
                        SetUnixOwner(file.FullName, "glados");
                    }

                    foreach (DirectoryInfo directory in subDirectories)
                    {
                        SetUnixOwner(directory.FullName, "glados");
                    }
                }
            }
            else
            {
                _logger.LogInformation("Unix file setting not needed");
            }
        }
    }
}
