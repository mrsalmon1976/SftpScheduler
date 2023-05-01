using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Job;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public interface IJobExecuteOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(string hash);
    }

    public class JobExecuteOrchestrator : IJobExecuteOrchestrator
    {
        private readonly IExecuteJobCommand _executeJobCommand;

        public JobExecuteOrchestrator(IExecuteJobCommand executeJobCommand)
        {
            _executeJobCommand = executeJobCommand;
        }

        public async Task<IActionResult> Execute(string hash)
        {

            int jobId = UrlUtils.Decode(hash);

            try
            {
                await _executeJobCommand.ExecuteAsync(jobId);
                return new OkResult();
            }
            catch (InvalidOperationException ioe)
            {
                return new BadRequestObjectResult(ioe.Message);
            }
        }
    }
}
