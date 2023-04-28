using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
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
    public abstract class SaveJobAbstractCommand
    {
        public async virtual Task<DateTimeOffset> ScheduleJob(IScheduler scheduler, JobEntity jobEntity)
        {
            // define the job 
            IJobDetail job = JobBuilder.Create<TransferJob>()
                .WithIdentity(TransferJob.GetJobKeyName(jobEntity.Id), TransferJob.DefaultGroup)
                .Build();

            // Trigger the job 

            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity(TransferJob.GetTriggerKeyName(jobEntity.Id), TransferJob.DefaultGroup)
              .WithCronSchedule(jobEntity.Schedule)
              .Build();

            return await scheduler.ScheduleJob(job, trigger);
        }

        public virtual void ValidateAndPrepareJobEntity(IDbContext dbContext, IJobValidator jobValidator, JobEntity jobEntity)
        {
            ValidationResult validationResult = jobValidator.Validate(dbContext, jobEntity);
            if (!validationResult.IsValid)
            {
                throw new DataValidationException("Job details supplied are invalid", validationResult);
            }

            // set the cached/calculated values
            jobEntity.ScheduleInWords = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(jobEntity.Schedule);

            // ensure remote paths are always suffixed and prefixed with "/"
            jobEntity.RemotePath = $"/{jobEntity.RemotePath.Trim('/')}/";

            if (!String.IsNullOrWhiteSpace(jobEntity.RemoteArchivePath))
            {
                jobEntity.RemoteArchivePath = $"/{jobEntity.RemoteArchivePath.Trim('/')}/";
            }

        }
    }
}
