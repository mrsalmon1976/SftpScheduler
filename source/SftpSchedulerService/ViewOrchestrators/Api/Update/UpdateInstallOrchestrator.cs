using Microsoft.AspNetCore.Mvc;
using SftpSchedulerService.Common.Diagnostics;
using SftpSchedulerService.Common;
using SftpSchedulerService.Config;

namespace SftpSchedulerService.ViewOrchestrators.Api.Update
{
    public interface IUpdateInstallOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute();
    }

    public class UpdateInstallOrchestrator : IUpdateInstallOrchestrator
    {
        private readonly ILogger<UpdateInstallOrchestrator> _logger;
        private readonly IProcessWrapperFactory _processWrapperFactory;
        private readonly AppSettings _appSettings;

        public UpdateInstallOrchestrator(ILogger<UpdateInstallOrchestrator> logger, IProcessWrapperFactory processWrapperFactory, AppSettings appSettings)
        {
            _logger = logger;
            _processWrapperFactory = processWrapperFactory;
            _appSettings = appSettings;
        }

        public async Task<IActionResult> Execute()
        {
            string autoUpdaterFolder = Path.Combine(_appSettings.BaseDirectory, UpdateConstants.AutoUpdaterFolderName);
            _logger.LogInformation("Autoupdater folder: {autoUpdaterFolder}", autoUpdaterFolder);

            using (IProcessWrapper process = _processWrapperFactory.CreateProcess())
            {
                process.StartInfo.WorkingDirectory = autoUpdaterFolder;
                process.StartInfo.FileName = UpdateConstants.AutoUpdaterExeFileName;
                process.StartInfo.Verb = UpdateConstants.StartInfoVerb;
                bool isStarted = process.Start();
                _logger.LogInformation("Process start result {isStarted}", isStarted);
            }

            return new OkResult();
        }
    }
}
