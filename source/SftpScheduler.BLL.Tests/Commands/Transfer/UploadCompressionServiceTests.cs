using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Models;
using SftpScheduler.Common.IO;
using SftpScheduler.Test.Common;
using System.IO.Compression;

namespace SftpScheduler.BLL.Tests.Commands.Transfer
{
    [TestFixture]
    public class UploadCompressionServiceTests
    {
        private static string TempDataFolder = Path.Combine(AppContext.BaseDirectory, "TempTestData");

        [SetUp]
        public void SetUp_UploadCompressionServiceTests()
        {
            Directory.CreateDirectory(TempDataFolder);
        }

        [TearDown]
        public void TearDown_UploadCompressionServiceTests()
        {
            Directory.Delete(TempDataFolder, true);
        }

        [Test]
        public void PrepareUploadFile_NoCompression_ReturnsOnlyOriginalInformation()
        {
            // setup
            string filePath = Path.Combine(TempDataFolder, Path.GetRandomFileName());
            IDirectoryUtility dirUtility = new SubstituteBuilder<IDirectoryUtility>().Build();

            // execute
            IUploadCompressionService uploadCompressionService = CreateUploadCompressionService(dirUtility: dirUtility);
            uploadCompressionService.PrepareUploadFile(filePath, Models.CompressionMode.None);

            // assert
            dirUtility.Received(1).EnsureExists(TempDataFolder);

            string[] files = Directory.GetFiles(TempDataFolder);
            Assert.That(files.Length, Is.EqualTo(0));   

        }

        [Test]
        public void PrepareUploadFile_ZipCompression_CreatesZipFile()
        {
            // setup
            string filePath = Path.Combine(TempDataFolder, Path.GetRandomFileName());
            File.WriteAllText(filePath, "This is a file that will be zipped!!!!");
            IDirectoryUtility dirUtility = new SubstituteBuilder<IDirectoryUtility>().Build();

            // execute
            IUploadCompressionService uploadCompressionService = CreateUploadCompressionService(dirUtility: dirUtility);
            CompressedFileInfo fileInfo = uploadCompressionService.PrepareUploadFile(filePath, Models.CompressionMode.Zip);

            // assert
            dirUtility.Received(1).EnsureExists(TempDataFolder);

            Assert.That(File.Exists(fileInfo.CompressedFilePath), Is.True);

            string expectedFileName = Path.GetFileNameWithoutExtension(filePath) + ".zip";
            string actualFileName = Path.GetFileName(fileInfo.CompressedFilePath!);
            Assert.That(actualFileName, Is.EqualTo(expectedFileName));

            using (ZipArchive zip = ZipFile.Open(fileInfo.CompressedFilePath!, ZipArchiveMode.Read))
            {
                Assert.That(zip.Entries.Count, Is.EqualTo(1));
            }

        }

        [Test]
        public void RemoveCompressedFile_CompressedFilePathIsNull_DoesNotDelete()
        {
            // setup
            CompressedFileInfo compressedFileInfo = new SubstituteBuilder<CompressedFileInfo>()
                .WithRandomProperties()
                .WithProperty(x => x.CompressedFilePath, null)
                .Build();
            IFileUtility fileUtility = new SubstituteBuilder<IFileUtility>().Build();

            // execute
            IUploadCompressionService uploadCompressionService = CreateUploadCompressionService(fileUtility: fileUtility);
            uploadCompressionService.RemoveCompressedFile(compressedFileInfo);

            // assert
            fileUtility.DidNotReceive().Exists(Arg.Any<string>());
            fileUtility.DidNotReceive().Delete(Arg.Any<string>());
        }

        [Test]
        public void RemoveCompressedFile_CompressedFileDoesNotExist_DoesNotDelete()
        {
            // setup
            CompressedFileInfo compressedFileInfo = new SubstituteBuilder<CompressedFileInfo>()
                .WithRandomProperties()
                .Build();
            IFileUtility fileUtility = new SubstituteBuilder<IFileUtility>().Build();
            fileUtility.Exists(compressedFileInfo.CompressedFilePath).Returns(false);

            // execute
            IUploadCompressionService uploadCompressionService = CreateUploadCompressionService(fileUtility: fileUtility);
            uploadCompressionService.RemoveCompressedFile(compressedFileInfo);

            // assert
            fileUtility.Received(1).Exists(compressedFileInfo.CompressedFilePath);
            fileUtility.DidNotReceive().Delete(Arg.Any<string>());

        }

        [Test]
        public void RemoveCompressedFile_CompressedFileExists_DeleteCalled()
        {
            // setup
            CompressedFileInfo compressedFileInfo = new SubstituteBuilder<CompressedFileInfo>()
                .WithRandomProperties()
                .Build();
            IFileUtility fileUtility = new SubstituteBuilder<IFileUtility>().Build();
            fileUtility.Exists(compressedFileInfo.CompressedFilePath).Returns(true);

            // execute
            IUploadCompressionService uploadCompressionService = CreateUploadCompressionService(fileUtility: fileUtility);
            uploadCompressionService.RemoveCompressedFile(compressedFileInfo);

            // assert
            fileUtility.Received(1).Delete(compressedFileInfo.CompressedFilePath!);
        }

        private IUploadCompressionService CreateUploadCompressionService(IDirectoryUtility? dirUtility = null, IFileUtility? fileUtility = null)
        {
            dirUtility ??= new SubstituteBuilder<IDirectoryUtility>().Build();
            fileUtility ??= new SubstituteBuilder<IFileUtility>().Build();
            return new UploadCompressionService(TempDataFolder, dirUtility, fileUtility);
        }
    }
}
