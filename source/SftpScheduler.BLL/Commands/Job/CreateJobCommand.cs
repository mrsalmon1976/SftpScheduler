using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Job
{
    public class CreateJobCommand
    {
        private readonly IJobValidator _jobValidator;
        private readonly ISchedulerFactory _schedulerFactory;

        public CreateJobCommand(IJobValidator jobValidator, ISchedulerFactory schedulerFactory) 
        {
            _jobValidator = jobValidator;
            _schedulerFactory = schedulerFactory;
        }
        
        public virtual async Task<JobEntity> ExecuteAsync(IDbContext dbContext, JobEntity jobEntity)
        {
            ValidationResult validationResult = _jobValidator.Validate(dbContext, jobEntity);
            if (!validationResult.IsValid)
            {
                throw new DataValidationException("Job details supplied are invalid", validationResult);
            }

            string sql = @"INSERT INTO Job (Name, HostId, Type, Schedule, LocalPath, RemotePath, Created) VALUES (@Name, @HostId, @Type, @Schedule, @LocalPath, @RemotePath, @Created)";
            await dbContext.ExecuteNonQueryAsync(sql, jobEntity);

            sql = @"select last_insert_rowid()";
            jobEntity.Id = await dbContext.ExecuteScalarAsync<int>(sql);

            IScheduler scheduler = await _schedulerFactory.GetScheduler();

            // define the job 
            IJobDetail job = JobBuilder.Create<TransferJob>()
                .WithIdentity(TransferJob.GetJobKeyName(jobEntity.Id), TransferJob.DefaultGroup)
                .Build();

            // Trigger the job 
            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity($"Trigger.{jobEntity.Id}", TransferJob.DefaultGroup)
              .WithCronSchedule(jobEntity.Schedule)
              .Build();
            await scheduler.ScheduleJob(job, trigger);

            return jobEntity;

        }
    }
}
