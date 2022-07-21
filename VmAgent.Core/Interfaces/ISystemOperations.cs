// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISystemOperations
    {
        DateTime UtcNow { get; }

        void ExtractToDirectory(string sourcePath, string targetPath);

        void CreateZipFile(string sourceDirectory, string destinationFilename);

        Task Delay(int milliseconds);
        
        Task Delay(int milliseconds, CancellationToken cancellationToken);

        Task Delay(TimeSpan timeSpan);

        void CopyDirectoryContents(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory);

        Guid NewGuid();

        void CreateDirectory(string fullPath);

        void FileAppendAllText(string path, string contents);
        
        void FileWriteAllText(string path, string contents);

        Task FileWriteAllBytesAsync(string path, byte[] contents, CancellationToken cancellationToken = default(CancellationToken));

        Task FileWriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default(CancellationToken));

        void DeleteDirectoryIfExists(string pathToDirectory);

        bool IsDirectoryEmpty(string pathToDirectory);

        void DeleteFile(string filePath);

        long FileInfoLength(string path);

        bool FileExists(string path);

        string FileReadAllText(string path);

        void FileCopy(string sourceFilePath, string destinationFilePath);

        void FileMoveWithOverwrite(string sourceFilePath, string destinationFilePath);
        
        void ExitProcess(int exitCode);

        (int exitCode, string stdOut, string stdErr) RunProcessWithStdCapture(Process process);

        int RunProcessWithoutStdCapture(Process process);

        bool IsOSPlatform(OSPlatform platform);

        void InstallCertificate(byte[] certificateContent);

        void UninstallCertificate(byte[] certificateContent);

        string GetHostName();

        IPHostEntry GetHostEntry(string hostName);

        Stream OpenFileForRead(string filePath);

        void SetUnixFilePermissions(string filePath, int permissions);

        void SetUnixOwnerIfNeeded(string path, bool applyToAllContents = false);

        bool DirectoryExists(string directoryPath);

        IEnumerable<FileInfo> GetFiles(DirectoryInfo source, bool recursive);

        IReadOnlyCollection<string> ListProcesses();
    }
}
