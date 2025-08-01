using SftpScheduler.Common.IO;
using SftpScheduler.Common;
using SftpSchedulerService.Config;

namespace SftpSchedulerService.Workers
{
	public interface ITempDataCleanupWorkerService
	{
		bool ExecuteCleanup();
	}

	public class TempDataCleanupWorkerService : ITempDataCleanupWorkerService
	{
		private readonly ILogger<TempDataCleanupWorkerService> _logger;
		private readonly AppSettings _appSettings;
		private readonly IDirectoryUtility _dirUtility;
		private readonly IFileUtility _fileUtility;

		public TempDataCleanupWorkerService(ILogger<TempDataCleanupWorkerService> logger, AppSettings appSettings, IDirectoryUtility dirUtility, IFileUtility fileUtility)
		{
			_logger = logger;
			_appSettings = appSettings;
			_dirUtility = dirUtility;
			_fileUtility = fileUtility;
		}

		public bool ExecuteCleanup()
		{
			string tempDataFolder = Path.Combine(_appSettings.TempDataDirectory);
			_logger.LogInformation("Scanning folder {tempDataFolder}", tempDataFolder);

			if (!_dirUtility.Exists(tempDataFolder))
			{
				_logger.LogInformation("Exiting as temporary data folder {tempDataFolder} does not exist", tempDataFolder);
				return true;
			}

			string[] tempFiles = _dirUtility.GetFiles(tempDataFolder, SearchOption.AllDirectories);
			if (tempFiles.Length == 0)
			{
				// no files found - we can exit
				_logger.LogInformation("Exiting - no files found in {tempDataFolder}", tempDataFolder);
				return true;
			}

			DateTime archiveDateTime = DateTime.UtcNow.AddDays(-2);

			foreach (string source in tempFiles)
			{
				try
				{
					FileInfo fileInfo = new FileInfo(source);
					if (fileInfo.LastAccessTimeUtc < archiveDateTime && fileInfo.LastWriteTimeUtc < archiveDateTime)
					{
						_fileUtility.Delete(source);
						_logger.LogInformation("Deleted temp file '{tempDataFolder}'", source);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning("Failed to delete temp file '{source}': {message}", source, ex.Message);
					return false;
				}
			}

			return true;
		}
	}
}
