using SftpScheduler.BLL.Models;
using SftpScheduler.Common.IO;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface IUploadCompressionService
    {
        CompressedFileInfo PrepareUploadFile(string filePath, SftpScheduler.BLL.Models.CompressionMode compressionMode);

        void RemoveCompressedFile(CompressedFileInfo compressedFileInfo);
    }
    public class UploadCompressionService : IUploadCompressionService
    {
        private readonly string _tempDataFolder;
        private readonly IDirectoryUtility _dirUtility;
        private readonly IFileUtility _fileUtility;

        public UploadCompressionService(string tempDataFolder, IDirectoryUtility dirUtility, IFileUtility fileUtility)
        {
            _tempDataFolder = tempDataFolder;
            _dirUtility = dirUtility;
            _fileUtility = fileUtility;
        }

        public CompressedFileInfo PrepareUploadFile(string filePath, SftpScheduler.BLL.Models.CompressionMode compressionMode)
        {
            CompressedFileInfo fileInfo = new CompressedFileInfo();
            fileInfo.OriginalFilePath = filePath;
            fileInfo.CompressionMode = compressionMode;

            _dirUtility.EnsureExists(_tempDataFolder);

            if (compressionMode == Models.CompressionMode.Zip) 
            {
                // make sure it isn't already a zipfile
                if (!IsZipFile(filePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    string fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
                    string zipPath = Path.Combine(_tempDataFolder, fileNameNoExt + ".zip");
                    using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    {
                        zip.CreateEntryFromFile(filePath, fileName);
                    }
                    fileInfo.CompressedFilePath = zipPath;
                }
            }

            return fileInfo;
        }

        public void RemoveCompressedFile(CompressedFileInfo compressedFileInfo)
        {
            string? filePath = compressedFileInfo.CompressedFilePath;
            if (filePath != null && _fileUtility.Exists(filePath))
            {
                _fileUtility.Delete(filePath);
            }
        }


        private static bool IsZipFile(string path)
        {
            try
            {
                using (var zipFile = ZipFile.OpenRead(path))
                {
                    var entries = zipFile.Entries;
                    return true;
                }
            }
            catch (InvalidDataException)
            {
                return false;
            }
        }
    }
}
