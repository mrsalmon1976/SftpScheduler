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
    public class CreateJobCommand : SaveJobAbstractCommand
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
            base.ValidateAndPrepareJobEntity(dbContext, _jobValidator, jobEntity);

            string sql = @"INSERT INTO Job (Name, HostId, Type, Schedule, ScheduleInWords, LocalPath, RemotePath, DeleteAfterDownload, RemoteArchivePath, LocalCopyPaths, IsEnabled, Created) VALUES (@Name, @HostId, @Type, @Schedule, @ScheduleInWords, @LocalPath, @RemotePath, @DeleteAfterDownload, @RemoteArchivePath, @LocalCopyPaths, @IsEnabled, @Created)";
            await dbContext.ExecuteNonQueryAsync(sql, jobEntity);

            sql = @"select last_insert_rowid()";
            jobEntity.Id = await dbContext.ExecuteScalarAsync<int>(sql);

            if (jobEntity.IsEnabled)
            {
                IScheduler scheduler = await _schedulerFactory.GetScheduler();
                await base.ScheduleJob(scheduler, jobEntity); 
            }

            return jobEntity;

        }
    }
}
