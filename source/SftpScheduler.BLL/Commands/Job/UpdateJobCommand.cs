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

            await base.UpdateJobSchedule(_schedulerFactory, jobEntity);

            return jobEntity;

        }
    }
}
