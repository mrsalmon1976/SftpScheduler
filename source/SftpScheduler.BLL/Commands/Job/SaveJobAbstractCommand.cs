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
        public async virtual Task<bool> UpdateJobSchedule(ISchedulerFactory schedulerFactory, JobEntity jobEntity)
        {

            IScheduler scheduler = await schedulerFactory.GetScheduler();
            JobKey jobKey = new JobKey(TransferJob.JobKeyName(jobEntity.Id), TransferJob.GroupName);
            bool isExistingJob = await scheduler.CheckExists(jobKey);

            // if there is an existing job, just delete it no matter what
            if (isExistingJob)
            {
                await scheduler.DeleteJob(jobKey);
            }

            // create a new schedule only if the job is enabled
            if (jobEntity.IsEnabled)
            {

                // define the job 
                IJobDetail job = JobBuilder.Create<TransferJob>()
                    .WithIdentity(jobKey)
                    .Build();

                // Trigger the job 
                ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity(TransferJob.TriggerKeyName(jobEntity.Id), TransferJob.GroupName)
                  .WithCronSchedule(jobEntity.Schedule)
                  .Build();


                await scheduler.ScheduleJob(job, trigger);
                return true;
            }

            return false;
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
