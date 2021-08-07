using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VmAgent.Core.Interfaces
{
    public interface IFileSystemOperations
    {
        void Create(DirectoryInfo directoryInfo);

        bool Exists(FileSystemInfo fileSystemInfo);

        bool TryGetParentDirectory(FileSystemInfo fileSystemInfo, out DirectoryInfo parentDirectory);

        void ExtractToDirectory(string sourcePath, string targetPath);

        void CreateZipFile(string sourceDirectory, string destinationFilename);

        IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo);

        IEnumerable<DirectoryInfo> GetDirectories(DirectoryInfo directoryInfo);

        IEnumerable<FileSystemInfo> GetFileSystemInfos(DirectoryInfo directoryInfo);

        void CopyTo(FileInfo toCopy, string targetPath, bool overwrite);

        void AppendAllText(string path, string content);

        void WriteAllText(string path, string content);

        void MoveWithOverwrite(string sourceFilePath, string destinationFilePath);

        Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken);

        Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken);

        void SetUnixOwner(string filePath, string owner);

        bool IsDirectory(string path);

        void Delete(FileSystemInfo fileSystemInfo);
    }
}
