using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.DataAnnotations;
using SftpSchedulerService.Models.Cron;

namespace SftpSchedulerService.ViewOrchestrators.Api.Cron
{
    public interface ICronGetScheduleOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(string schedule);
    }

    public class CronGetScheduleOrchestrator : ICronGetScheduleOrchestrator
    {
        private readonly ILogger<CronGetScheduleOrchestrator> _logger;

        public CronGetScheduleOrchestrator(ILogger<CronGetScheduleOrchestrator> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Execute(string schedule)
        {
            CronResult result = new CronResult();

            // validate first
            try
            {
                CronScheduleAttribute scheduleAttribute = new CronScheduleAttribute();
                scheduleAttribute.Validate(schedule, "Cron");

                string scheduleInWords = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(schedule);
                result.IsValid = true;
                result.ScheduleInWords = scheduleInWords;

            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                result.ErrorMessage = ex.Message;
            }
            catch (FormatException ex)
            {
                result.ErrorMessage = "Cron schedule is invalid or not supported";
                _logger.LogWarning(ex.Message);
            }

            return await Task.FromResult(new OkObjectResult(result));
        }

    }
}
