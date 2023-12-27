using SftpScheduler.BLL.Models;
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

    }
    public class UploadCompressionService : IUploadCompressionService
    {
        public CompressedFileInfo PrepareUploadFile(string filePath, SftpScheduler.BLL.Models.CompressionMode compressionMode)
        {
            CompressedFileInfo fileInfo = new CompressedFileInfo();
            fileInfo.OriginalFilePath = filePath;
            fileInfo.CompressionMode = compressionMode;

            string originalFileFolder = Path.GetDirectoryName(filePath)!;

            if (compressionMode == Models.CompressionMode.Zip) 
            {
                // make sure it isn't already a zipfile
                if (!IsZipFile(filePath))
                {
                    string zipPath = Path.Combine(originalFileFolder, Path.GetRandomFileName() + ".zip");
                    string fileName = Path.GetFileName(filePath);
                    using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    {
                        zip.CreateEntryFromFile(filePath, fileName);
                    }
                    fileInfo.CompressedFilePath = zipPath;
                }
            }

            return fileInfo;
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
