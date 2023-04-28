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
    public interface IUpdateJobCommand
    {
        Task<JobEntity> ExecuteAsync(IDbContext dbContext, JobEntity jobEntity);
    }

    public class UpdateJobCommand : SaveJobAbstractCommand, IUpdateJobCommand
    {
        private readonly IJobValidator _jobValidator;
        private readonly ISchedulerFactory _schedulerFactory;

        public UpdateJobCommand(IJobValidator jobValidator, ISchedulerFactory schedulerFactory) 
        {
            _jobValidator = jobValidator;
            _schedulerFactory = schedulerFactory;
        }
        
        public virtual async Task<JobEntity> ExecuteAsync(IDbContext dbContext, JobEntity jobEntity)
        {
            base.ValidateAndPrepareJobEntity(dbContext, _jobValidator, jobEntity);

            string sql = @"UPDATE Job 
                SET Name = @Name
                , HostId = @HostId
                , Type = @Type
                , Schedule = @Schedule
                , ScheduleInWords = @ScheduleInWords
                , LocalPath = @LocalPath
                , RemotePath = @RemotePath
                , DeleteAfterDownload = @DeleteAfterDownload
                , RemoteArchivePath = @RemoteArchivePath
                , LocalCopyPaths = @LocalCopyPaths
                , IsEnabled = @IsEnabled 
                WHERE Id = @Id";
            await dbContext.ExecuteNonQueryAsync(sql, jobEntity);

            IScheduler scheduler = await _schedulerFactory.GetScheduler();
            JobKey jobKey = new JobKey(TransferJob.GetJobKeyName(jobEntity.Id), TransferJob.DefaultGroup);
            bool isExistingJob = await scheduler.CheckExists(jobKey);

            // if there is an existing job, just delete it not matter what
            if (isExistingJob)
            {
                await scheduler.DeleteJob(jobKey);
            }

            // create a new schedule only if the job is enabled
            if (jobEntity.IsEnabled)
            {
                await base.ScheduleJob(scheduler, jobEntity); 
            }

            return jobEntity;

        }
    }
}
