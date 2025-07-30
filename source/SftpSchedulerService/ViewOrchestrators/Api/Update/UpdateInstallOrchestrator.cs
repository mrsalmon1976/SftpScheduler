using Microsoft.AspNetCore.Mvc;
using SftpScheduler.Common;
using SftpScheduler.Common.Diagnostics;
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
            string scriptPath = Path.Combine(_appSettings.BaseDirectory, UpdateConstants.UpdaterFileName);
            _logger.LogInformation("Attempting to run update: {scriptPath}", scriptPath);

            using (IProcessWrapper process = _processWrapperFactory.CreateProcess())
            {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = _appSettings.BaseDirectory;
                process.StartInfo.Verb = UpdateConstants.StartInfoVerb;
                bool isStarted = process.Start();
                _logger.LogInformation("Process start result: {isStarted}", isStarted);
            }
            return await Task.FromResult(new OkResult());
        }
    }
}
