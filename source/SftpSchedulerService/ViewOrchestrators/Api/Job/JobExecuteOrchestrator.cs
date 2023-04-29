using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Utilities;
using System.Data;
using System.Security.Policy;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public class JobExecuteOrchestrator
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
