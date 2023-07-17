using Microsoft.Azure.Gaming.VmAgent.Core;
using Microsoft.Azure.Gaming.VmAgent.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;
using VmAgent.Core.Interfaces;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class SystemOperationTests
    {
        private string _root = "C:\\";
        private string _directoryPath;
        private string _subdirectoryPath;
        private string _directoryFilePath;
        private string _subdirectoryFilePath;
        private string _subdirectoryName = "subdirectory";
        private string _directoryName = "directory";
        private string _directoryFileName = "file";
        private string _subdirectoryFileName = "subfile";
        private string _defaultFileContent = "This is not the file you are looking for";
        private string _defaultSourceFile; 
        private DirectoryInfo _subDirectoryInfo;
        private DirectoryInfo _directoryInfo;
        private SystemOperations _systemOperations;
        private VmConfiguration _vmConfiguration;
        private MultiLogger _logger;
        private Mock<IFileSystemOperations> _mockFileSystemOperations;

        [TestInitialize]
        public void BeforeEachTest()
        {
            _directoryPath = Path.Combine(_root, _directoryName);
            _directoryInfo = new DirectoryInfo(_directoryPath);
            _subdirectoryPath = Path.Combine(_directoryPath, _subdirectoryName);
            _subDirectoryInfo = new DirectoryInfo(_subdirectoryPath);
            _directoryFilePath = Path.Combine(_directoryPath, _directoryFileName);
            FileInfo directoryFileInfo = new FileInfo(_directoryFilePath);
            _subdirectoryFilePath = Path.Combine(_subdirectoryPath, _subdirectoryFileName);
            FileInfo subDirectoryFileInfo = new FileInfo(_subdirectoryFilePath);
            _defaultSourceFile = Path.Combine(_root, "Source");

            _mockFileSystemOperations = new Mock<IFileSystemOperations>();
            _vmConfiguration = new VmConfiguration(56001, "vmid", new VmDirectories("root"), true);
            _logger = new MultiLogger(NullLogger.Instance);
            _systemOperations = new SystemOperations(_vmConfiguration, _logger, _mockFileSystemOperations.Object);

            _mockFileSystemOperations.Setup(x => x.IsDirectory(_root)).Returns(true);
            _mockFileSystemOperations.Setup(x => x.IsDirectory(_subdirectoryPath)).Returns(true);
            _mockFileSystemOperations.Setup(x => x.IsDirectory(_directoryPath)).Returns(true);
            _mockFileSystemOperations.Setup(x => x.IsDirectory(_directoryFilePath)).Returns(false);
            _mockFileSystemOperations.Setup(x => x.IsDirectory(_subdirectoryFilePath)).Returns(false);

            _mockFileSystemOperations.Setup(x => x.GetFiles(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _root)), It.IsAny<bool>()))
                .Returns<DirectoryInfo, bool>(
                (dirInfo, recursive) =>
                {
                    List<FileInfo> toReturn = new List<FileInfo>();
                    if (recursive)
                    {
                        toReturn.Add(subDirectoryFileInfo);
                        toReturn.Add(directoryFileInfo);
                    }
                    return toReturn;
                });

            _mockFileSystemOperations.Setup(x => x.GetFiles(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _directoryPath)), It.IsAny<bool>()))
                .Returns<DirectoryInfo, bool>(
                (dirInfo, recursive) =>
                {
                    List<FileInfo> toReturn = new List<FileInfo>();
                    toReturn.Add(directoryFileInfo);
                    if (recursive)
                    {
                        toReturn.Add(subDirectoryFileInfo);
                    }
                    return toReturn;
                });

            _mockFileSystemOperations.Setup(x => x.GetFiles(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _subdirectoryPath)), It.IsAny<bool>()))
               .Returns<DirectoryInfo, bool>(
                (dirInfo, recursive) =>
                {
                    List<FileInfo> toReturn = new List<FileInfo>();
                    toReturn.Add(subDirectoryFileInfo);
                    return toReturn;
                });

            _mockFileSystemOperations.Setup(x => x.GetDirectories(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _root)), It.IsAny<bool>()))
               .Returns<DirectoryInfo, bool>(
               (dirInfo, recursive) =>
               {
                   List<DirectoryInfo> toReturn = new List<DirectoryInfo>();
                   toReturn.Add(_directoryInfo);
                   if (recursive)
                   {
                       toReturn.Add(_subDirectoryInfo);
                   }
                   return toReturn;
               });

            _mockFileSystemOperations.Setup(x => x.GetDirectories(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _directoryPath)), It.IsAny<bool>()))
                .Returns<DirectoryInfo, bool>(
                (dirInfo, recursive) =>
                {
                    List<DirectoryInfo> toReturn = new List<DirectoryInfo>();
                    toReturn.Add(_subDirectoryInfo);
                    return toReturn;
                });

            _mockFileSystemOperations.Setup(x => x.GetDirectories(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _subdirectoryPath)), It.IsAny<bool>()))
               .Returns<DirectoryInfo, bool>(
                (dirInfo, recursive) =>
                {
                    List<DirectoryInfo> toReturn = new List<DirectoryInfo>();
                    return toReturn;
                });

            _mockFileSystemOperations.Setup(x => x.GetFileSystemInfos(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _root)), It.IsAny<bool>()))
               .Returns<DirectoryInfo, bool>(
               (dirInfo, recursive) =>
               {
                   List<FileSystemInfo> toReturn = new List<FileSystemInfo>();
                   toReturn.Add(_directoryInfo);
                   if (recursive)
                   {
                       toReturn.Add(_subDirectoryInfo);
                       toReturn.Add(directoryFileInfo);
                       toReturn.Add(subDirectoryFileInfo);
                   }
                   return toReturn;
               });

            _mockFileSystemOperations.Setup(x => x.GetFileSystemInfos(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _directoryPath)), It.IsAny<bool>()))
                .Returns<DirectoryInfo, bool>(
                (dirInfo, recursive) =>
                {
                    List<FileSystemInfo> toReturn = new List<FileSystemInfo>();
                    toReturn.Add(_subDirectoryInfo);
                    toReturn.Add(directoryFileInfo);
                    if (recursive)
                    {
                        toReturn.Add(subDirectoryFileInfo);
                    }
                    return toReturn;
                });

            _mockFileSystemOperations.Setup(x => x.GetFileSystemInfos(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _subdirectoryPath)), It.IsAny<bool>()))
               .Returns<DirectoryInfo, bool>(
                (dirInfo, recursive) =>
                {
                    List<FileSystemInfo> toReturn = new List<FileSystemInfo>();
                    toReturn.Add(subDirectoryFileInfo);
                    return toReturn;
                });

            _mockFileSystemOperations.Setup(x => x.CreateSubdirectory(It.IsAny<DirectoryInfo>(), It.IsAny<string>()))
                .Returns<DirectoryInfo, string>((info, name) => new DirectoryInfo(Path.Combine(info.FullName, name)));

            DirectoryInfo nullDirInfo = null;
            DirectoryInfo rootDirInfo = new DirectoryInfo(_root);
            _mockFileSystemOperations.Setup(x => x.TryGetParentDirectory(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _root)), out nullDirInfo)).Returns(false);
            _mockFileSystemOperations.Setup(x => x.TryGetParentDirectory(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _directoryPath)), out rootDirInfo)).Returns(true);
            _mockFileSystemOperations.Setup(x => x.TryGetParentDirectory(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _subdirectoryPath)), out _directoryInfo)).Returns(true);
            _mockFileSystemOperations.Setup(x => x.Exists(It.Is<FileSystemInfo>((info) => IsFileSystemInfoEqual((DirectoryInfo)info, _root)))).Returns(true);
            _mockFileSystemOperations.Setup(x => x.Exists(It.IsAny<FileSystemInfo>())).Returns(false);
        }

        bool IsFileSystemInfoEqual(FileSystemInfo toCompare, string path)
        {
            return toCompare.FullName == path;
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestSetUnixOwnerRecursive()
        {
            List<string> expectedItemsToOwn = new List<string>()
            {
                _directoryPath,
                _subdirectoryPath,
                _directoryFilePath,
                _subdirectoryFilePath
            };

            _systemOperations.SetUnixOwnerIfNeeded(_directoryPath, true);

            expectedItemsToOwn.ForEach(
                    (item) => _mockFileSystemOperations.Verify(x => x.SetUnixOwner(It.Is<string>((path) => path == item), _systemOperations.User), Times.Once));
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestSetUnixOwnerNonRecursive()
        {
            List<string> unexpectedItemsToOwn = new List<string>()
            {
                _subdirectoryPath,
                _directoryFilePath,
                _subdirectoryFilePath
            };

            _systemOperations.SetUnixOwnerIfNeeded(_directoryPath, false);

            unexpectedItemsToOwn.ForEach(
                    (item) => _mockFileSystemOperations.Verify(x => x.SetUnixOwner(It.Is<string>((path) => path == item), _systemOperations.User), Times.Never));
            _mockFileSystemOperations.Verify(x => x.SetUnixOwner(It.Is<string>((path) => path.EndsWith(_directoryPath)), _systemOperations.User), Times.Once);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestSetUnixOwnerDoesNothing()
        {
            List<string> expectedItemsToOwn = new List<string>()
            {
                _directoryPath,
                _subdirectoryPath,
                _directoryFilePath,
                _subdirectoryFilePath
            };
            VmConfiguration config = new VmConfiguration(56001, "vmid", new VmDirectories("root"), false);
            SystemOperations systemOperations = new SystemOperations(config, _logger, _mockFileSystemOperations.Object);
            systemOperations.SetUnixOwnerIfNeeded(_directoryPath, true);
            expectedItemsToOwn.ForEach(
                    (item) => _mockFileSystemOperations.Verify(x => x.SetUnixOwner(It.IsAny<string>(), It.IsAny<string>()), Times.Never));
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestCreateParentStructure()
        {
            _systemOperations.CreateDirectoryAndParents(_subDirectoryInfo);
            SubdirectoryVerifyDirectoryCreationAndOwnership();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestCopyDirectoryContents()
        {
            string targetDir = Path.Combine(_root, "copy");
            List<string> expectedDirectoriesToCreateOwn = new List<string>()
            {
                Path.Combine(targetDir, _subdirectoryName),
                targetDir
            };
            List<(string, string)> expectedFilesToCreateOwn = new List<(string, string)>()
            {
                (Path.Combine(targetDir, _directoryFileName), _directoryFilePath),
                (Path.Combine(targetDir, _subdirectoryName, _subdirectoryFileName), _subdirectoryFilePath)
            };
            List<string> unexpectedItemsToModify = new List<string>()
            {
                _directoryPath,
                _subdirectoryPath,
                _directoryFilePath,
                _subdirectoryFilePath
            };

            _systemOperations.CopyDirectoryContents(_directoryInfo, new DirectoryInfo(targetDir));
            expectedDirectoriesToCreateOwn.ForEach(
                    (item) => {
                        _mockFileSystemOperations.Verify(x => x.SetUnixOwner(It.Is<string>((path) => path == item), _systemOperations.User), Times.Once);
                        _mockFileSystemOperations.Verify(x => x.Create(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, item))), Times.Once);
                    });

            expectedFilesToCreateOwn.ForEach(
                   (item) => {
                       _mockFileSystemOperations.Verify(x => x.SetUnixOwner(It.Is<string>((path) => path == item.Item1), _systemOperations.User), Times.Once);
                       _mockFileSystemOperations.Verify(x => x.CopyTo(It.Is<FileInfo>((info) => IsFileSystemInfoEqual(info, item.Item2)), item.Item1, true), Times.Once);
                   });
            unexpectedItemsToModify.ForEach(
                    (item) => {
                        _mockFileSystemOperations.Verify(x => x.SetUnixOwner(It.Is<string>((path) => path == item), _systemOperations.User), Times.Never);
                        _mockFileSystemOperations.Verify(x => x.Create(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, item))), Times.Never);
                    });
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void FileCopyTest()
        {
            _systemOperations.FileCopy(_defaultSourceFile, _subdirectoryFilePath);
           
            _mockFileSystemOperations.Verify(x => x.CopyTo(It.Is<FileInfo>((info) => IsFileSystemInfoEqual(info, _defaultSourceFile)), _subdirectoryFilePath, true), Times.Once);
            SubdirectoryVerifyDirectoryCreationAndOwnership();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void CreateDirectoryTest()
        {
            _systemOperations.CreateDirectory(_subdirectoryPath);
            SubdirectoryVerifyDirectoryCreationAndOwnership();
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void FileWriteAllTextTest()
        { 
            _systemOperations.FileWriteAllText(_subdirectoryFilePath, _defaultFileContent);
            SubdirectoryVerifyDirectoryCreationAndOwnership();
            _mockFileSystemOperations.Verify(x => x.WriteAllText(_subdirectoryFilePath, _defaultFileContent), Times.Once);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void FileAppendAllTextTest()
        {
            _systemOperations.FileAppendAllText(_subdirectoryFilePath, _defaultFileContent);
            SubdirectoryVerifyDirectoryCreationAndOwnership();
            _mockFileSystemOperations.Verify(x => x.AppendAllText(_subdirectoryFilePath, _defaultFileContent), Times.Once);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void FileMoveWithOverwriteTest()
        {
            _systemOperations.FileMoveWithOverwrite(_defaultSourceFile, _subdirectoryFilePath);
            SubdirectoryVerifyDirectoryCreationAndOwnership();
            _mockFileSystemOperations.Verify(x => x.MoveWithOverwrite(_defaultSourceFile, _subdirectoryFilePath), Times.Once);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public async Task FileWriteAllBytesAsyncTest()
        {
            await _systemOperations.FileWriteAllBytesAsync(_subdirectoryFilePath, new byte[10]);
            SubdirectoryVerifyDirectoryCreationAndOwnership();
            _mockFileSystemOperations.Verify(x => x.WriteAllBytesAsync(_subdirectoryFilePath, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public async Task FileWriteAllTextAsyncTest()
        {
            await _systemOperations.FileWriteAllTextAsync(_subdirectoryFilePath, _defaultFileContent);
            SubdirectoryVerifyDirectoryCreationAndOwnership();
            _mockFileSystemOperations.Verify(x => x.WriteAllTextAsync(_subdirectoryFilePath,_defaultFileContent, It.IsAny<CancellationToken>()), Times.Once);
        }

        private void SubdirectoryVerifyDirectoryCreationAndOwnership()
        {
            List<string> expectedDirectoriesToCreateOwn = new List<string>()
            {
                _subdirectoryPath,
                _directoryPath
            };
            VerifyDirectoryCreationAndOwnership(_subDirectoryInfo, expectedDirectoriesToCreateOwn);
        }

        private void VerifyDirectoryCreationAndOwnership(DirectoryInfo parentDirectory, List<string> expectedDirectoriesToCreateOwn)
        {
            expectedDirectoriesToCreateOwn.ForEach(
                    (item) => {
                        _mockFileSystemOperations.Verify(x => x.SetUnixOwner(It.Is<string>((path) => path == item), _systemOperations.User), Times.Once);
                        _mockFileSystemOperations.Verify(x => x.Create(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, item))), Times.Once);
                    });
            _mockFileSystemOperations.Verify(x => x.SetUnixOwner(It.Is<string>((path) => path == _root), _systemOperations.User), Times.Never);
            _mockFileSystemOperations.Verify(x => x.Create(It.Is<DirectoryInfo>((info) => IsFileSystemInfoEqual(info, _root))), Times.Never);
        }
    }
}
