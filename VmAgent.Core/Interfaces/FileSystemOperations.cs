using Mono.Unix.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VmAgent.Core.Interfaces
{
    public class FileSystemOperations : IFileSystemOperations
    {
        public void AppendAllText(string path, string content)
        {
            File.AppendAllText(path, content);
        }

        public void CopyTo(FileInfo toCopy, string targetPath, bool overwrite)
        {
            toCopy.CopyTo(targetPath, overwrite);
        }

        public void Create(DirectoryInfo directoryInfo)
        {
            directoryInfo.Create();
        }

        public void CreateZipFile(string sourceDirectory, string destinationFilename)
        {
            ZipFile.CreateFromDirectory(sourceDirectory, destinationFilename);
        }

        public void Delete(FileSystemInfo fileSystemInfo)
        {
            DirectoryInfo dirInfo = fileSystemInfo as DirectoryInfo;
            FileInfo fileInfo = fileSystemInfo as FileInfo;
            if (dirInfo != null)
            {
                Directory.Delete(dirInfo.FullName, true);
            }
            else if(fileInfo != null)
            {
                File.Delete(fileInfo.FullName);
            }
            else
            {
                throw new ArgumentException("This is not a file or a directory");
            }
        }

        public bool Exists(FileSystemInfo fileSystemInfo)
        {
            return fileSystemInfo.Exists;
        }

        public void ExtractToDirectory(string sourcePath, string targetPath)
        {
            ZipFile.ExtractToDirectory(sourcePath, targetPath);
        }

        public IEnumerable<DirectoryInfo> GetDirectories(DirectoryInfo directoryInfo, bool recursive = false)
        {
            return directoryInfo.EnumerateDirectories("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo, bool recursive = false)
        {
            return directoryInfo.EnumerateFiles("*", recursive ?  SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<FileSystemInfo> GetFileSystemInfos(DirectoryInfo directoryInfo, bool recursive = false)
        {
            return directoryInfo.EnumerateFileSystemInfos("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public bool TryGetParentDirectory(FileSystemInfo fileSystemInfo, out DirectoryInfo parentDirectory)
        {
            DirectoryInfo dirInfo = fileSystemInfo as DirectoryInfo;
            FileInfo fileInfo = fileSystemInfo as FileInfo;
            if (dirInfo != null)
            {
                parentDirectory = dirInfo.Parent;
                return dirInfo.Parent != null; 
            }
            else if (fileInfo != null)
            {
                parentDirectory = fileInfo.Directory;
                return true;
            }
            else
            {
                throw new ArgumentException("This is not a file or a directory");
            }
        }

        public bool IsDirectory(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public void MoveWithOverwrite(string sourceFilePath, string destinationFilePath)
        {
            File.Move(sourceFilePath, destinationFilePath, true);
        }

        public void SetUnixOwner(string filePath, string owner)
        {
            var passwd = Syscall.getpwnam(owner);
            if (passwd == null)
            {
                throw new ArgumentException($"The specified user {owner} could not be retrieved");
            }
            var result = Syscall.chown(filePath, passwd.pw_uid, passwd.pw_gid);
            if (result != 0)
            {
                throw new ArgumentException($"Failed to give ownership of {filePath} to {owner}");
            }
        }

        public Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken)
        {
            return File.WriteAllBytesAsync(path, content, cancellationToken);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken)
        {
            return File.WriteAllTextAsync(path, content, cancellationToken);
        }

        public DirectoryInfo CreateSubdirectory(DirectoryInfo parentDir, string subdirectoryName)
        {
            return parentDir.CreateSubdirectory(subdirectoryName);
        }
    }
}
