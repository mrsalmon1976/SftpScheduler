using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Services.Job;
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
        Task<JobEntity> ExecuteAsync(IDbContext dbContext, JobEntity jobEntity, string userName);
    }

    public class UpdateJobCommand : SaveJobAbstractCommand, IUpdateJobCommand
    {
        private readonly IJobValidator _jobValidator;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobAuditService _jobAuditService;
        private readonly JobRepository _jobRepository;

        public UpdateJobCommand(IJobValidator jobValidator, ISchedulerFactory schedulerFactory, IJobAuditService jobAuditService, JobRepository jobRepository) 
        {
            _jobValidator = jobValidator;
            _schedulerFactory = schedulerFactory;
            _jobAuditService = jobAuditService;
            _jobRepository = jobRepository;
        }
        
        public virtual async Task<JobEntity> ExecuteAsync(IDbContext dbContext, JobEntity jobEntity, string userName)
        {
            base.ValidateAndPrepareJobEntity(dbContext, _jobValidator, jobEntity);

            // run comparison to determine auditing
            JobEntity currentJobEntity = await _jobRepository.GetByIdAsync(dbContext, jobEntity.Id);
            var auditLogs = await _jobAuditService.CompareJobs(dbContext, currentJobEntity, jobEntity, userName);

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
                , RestartOnFailure = @RestartOnFailure
                WHERE Id = @Id";
            await dbContext.ExecuteNonQueryAsync(sql, jobEntity);

            // write audit logs
            sql = @"INSERT INTO JobAuditLog (JobId, PropertyName, FromValue, ToValue, UserName, Created) VALUES (@JobId, @PropertyName, @FromValue, @ToValue, @UserName, @Created)";
            foreach (JobAuditLogEntity jobAuditLogEntity in auditLogs)
            {
                await dbContext.ExecuteNonQueryAsync(sql, jobAuditLogEntity);
            }


            await base.UpdateJobSchedule(_schedulerFactory, jobEntity);

            return jobEntity;

        }
    }
}
