using SftpScheduler.Common.IO;
using SftpScheduler.Common;
using SftpSchedulerService.Config;

namespace SftpSchedulerService.Workers
{
	public interface IUpdateCleanupWorkerService
	{
		bool ExecuteCleanup();
	}

	public class UpdateCleanupWorkerService : IUpdateCleanupWorkerService
	{
		private readonly ILogger<UpdateCleanupWorkerService> _logger;
		private readonly AppSettings _appSettings;
		private readonly IDirectoryUtility _dirUtility;
		private readonly IFileUtility _fileUtility;

		public UpdateCleanupWorkerService(ILogger<UpdateCleanupWorkerService> logger, AppSettings appSettings, IDirectoryUtility dirUtility, IFileUtility fileUtility)
		{
			_logger = logger;
			_appSettings = appSettings;
			_dirUtility = dirUtility;
			_fileUtility = fileUtility;
		}

		public bool ExecuteCleanup()
		{
			string updaterPath = Path.Combine(_appSettings.BaseDirectory, UpdateConstants.UpdaterFolderName);
			_logger.LogInformation("Scanning folder {updaterPath}", updaterPath);

			if (!_dirUtility.Exists(updaterPath))
			{
				_logger.LogWarning("Exiting as Updater folder {updaterPath} does not exist", updaterPath);
				return true;
			}

			string[] tempFiles = _dirUtility.GetFiles(updaterPath, SearchOption.AllDirectories, "*" + UpdateConstants.UpdaterNewFileExtension);
			if (tempFiles.Length == 0)
			{
				// no files found - we can exit
				_logger.LogInformation("Exiting - no more temp files found in {updaterPath}", updaterPath);
				return true;
			}

			foreach (string source in tempFiles)
			{
				try
				{
					string target = Path.Combine(Path.GetDirectoryName(source)!, Path.GetFileNameWithoutExtension(source));
					_fileUtility.Move(source, target, true);
					_logger.LogInformation("Renamed temp file '{source}' to '{target}'", source, target);
				}
				catch (Exception ex)
				{
					_logger.LogWarning("Failed to rename temp file '{source}': {message}", source, ex.Message);
				}
			}

			return false;
		}
	}
}
