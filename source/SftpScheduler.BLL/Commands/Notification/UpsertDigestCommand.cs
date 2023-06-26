using Quartz;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Notification
{
    public interface IUpsertDigestCommand
    {
        Task Execute(IEnumerable<DayOfWeek> daysOfWeek, int hour);
    }

    public class UpsertDigestCommand : IUpsertDigestCommand
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly CronBuilder _cronBuilder;

        public UpsertDigestCommand(ISchedulerFactory schedulerFactory, CronBuilder cronBuilder)
        {
            _schedulerFactory = schedulerFactory;
            _cronBuilder = cronBuilder;
        }

        public virtual async Task Execute(IEnumerable<DayOfWeek> daysOfWeek, int hour)
        {
            IScheduler scheduler = await _schedulerFactory.GetScheduler();
            JobKey jobKey = new JobKey(DigestJob.JobKeyName(), DigestJob.GroupName);
            bool isExistingJob = await scheduler.CheckExists(jobKey);

            // if there is an existing job, just delete it no matter what
            if (isExistingJob)
            {
                await scheduler.DeleteJob(jobKey);
            }

            // only create the job if it is scheduled for specific days
            if (daysOfWeek.Any())
            {
                string schedule = _cronBuilder.Daily(daysOfWeek, hour)!;

                IJobDetail job = JobBuilder.Create<DigestJob>()
                    .WithIdentity(jobKey)
                    .Build();

                // schedule = "0 0/1 * ? * * *";

                ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity(DigestJob.TriggerKeyName(), DigestJob.GroupName)
                  .WithCronSchedule(schedule)
                  .Build();

                await scheduler.ScheduleJob(job, trigger);
            }

        }
    }
}
