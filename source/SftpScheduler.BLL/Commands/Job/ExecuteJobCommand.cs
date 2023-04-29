using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Job
{
    public interface IExecuteJobCommand
    {
        Task<bool> ExecuteAsync(int jobId);
    }

    public class ExecuteJobCommand : IExecuteJobCommand
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public ExecuteJobCommand(ISchedulerFactory schedulerFactory) 
        {
            _schedulerFactory = schedulerFactory;
        }

        public virtual async Task<bool> ExecuteAsync(int jobId)
        {
            string jobKeyName = $"AdHoc.Job.{jobId}";
            JobKey jobKey = new JobKey(jobKeyName, TransferJob.DefaultGroup);
            TriggerKey triggerKey = new TriggerKey($"AdHoc.Trigger.{jobId}", TransferJob.DefaultGroup);

            IScheduler scheduler = await _schedulerFactory.GetScheduler();
            
            IJobDetail? jobDetail = await scheduler.GetJobDetail(jobKey);
            if (jobDetail != null)
            {
                throw new InvalidOperationException("An ad hoc run for this job has already been scheduled and is not yet complete");
            }

            // define the job
            IJobDetail job = JobBuilder.Create<TransferJob>()
                .WithIdentity(jobKey)
                .Build();

            // Trigger the job 
            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity(triggerKey)
              .StartNow()
              .Build();

            await scheduler.ScheduleJob(job, trigger);

            return true;

        }
    }
}
