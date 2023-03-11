using Microsoft.AspNetCore.Mvc;
using SftpSchedulerService.Controllers.Api;
using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Cron;

namespace SftpSchedulerService.ViewOrchestrators.Api.Cron
{
    public class CronGetScheduleOrchestrator
    {
        private readonly ILogger<CronGetScheduleOrchestrator> _logger;

        public CronGetScheduleOrchestrator(ILogger<CronGetScheduleOrchestrator> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Execute(string schedule)
        {
            CronResult result = new CronResult();

            if (String.IsNullOrWhiteSpace(schedule))
            {
                result.ErrorMessage = "No cron schedule supplied";
            }
            else
            {
                try
                {
                    string scheduleInWords = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(schedule);
                    result.IsValid = true;
                    result.ScheduleInWords = scheduleInWords;
                }
                catch (FormatException ex)
                {
                    result.ErrorMessage = "Invalid cron schedule";
                    _logger.LogWarning(ex.Message);
                }
            }

            return await Task.FromResult(new OkObjectResult(result));
        }

    }
}
