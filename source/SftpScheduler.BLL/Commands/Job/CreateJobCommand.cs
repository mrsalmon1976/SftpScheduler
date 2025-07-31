using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;

namespace SftpScheduler.BLL.Commands.Job
{
    public interface ICreateJobCommand
    {
        Task<JobEntity> ExecuteAsync(IDbContext dbContext, JobEntity jobEntity, string userName);
    }

    public class CreateJobCommand : SaveJobAbstractCommand, ICreateJobCommand
    {
        private readonly IJobValidator _jobValidator;
        private readonly ISchedulerFactory _schedulerFactory;

        public CreateJobCommand(IJobValidator jobValidator, ISchedulerFactory schedulerFactory) 
        {
            _jobValidator = jobValidator;
            _schedulerFactory = schedulerFactory;
        }
        
        public virtual async Task<JobEntity> ExecuteAsync(IDbContext dbContext, JobEntity jobEntity, string userName)
        {
            base.ValidateAndPrepareJobEntity(dbContext, _jobValidator, jobEntity);

            string sql = @"INSERT INTO Job 
                (Name, HostId, Type, Schedule, ScheduleInWords, LocalPath, RemotePath, DeleteAfterDownload, RemoteArchivePath, LocalCopyPaths, IsEnabled, Created, RestartOnFailure, CompressionMode, FileMask, PreserveTimestamp, TransferMode, LocalArchivePath, LocalPrefix) 
                VALUES 
                (@Name, @HostId, @Type, @Schedule, @ScheduleInWords, @LocalPath, @RemotePath, @DeleteAfterDownload, @RemoteArchivePath, @LocalCopyPaths, @IsEnabled, @Created, @RestartOnFailure, @CompressionMode, @FileMask, @PreserveTimestamp, @TransferMode, @LocalArchivePath, @LocalPrefix)";
            await dbContext.ExecuteNonQueryAsync(sql, jobEntity);

            sql = @"select last_insert_rowid()";
            jobEntity.Id = await dbContext.ExecuteScalarAsync<int>(sql);

            // write audit logs
            sql = @"INSERT INTO JobAuditLog (JobId, PropertyName, FromValue, ToValue, UserName, Created) VALUES (@JobId, @PropertyName, @FromValue, @ToValue, @UserName, @Created)";
            await dbContext.ExecuteNonQueryAsync(sql, new JobAuditLogEntity()
            {
                JobId = jobEntity.Id,
                PropertyName = "Job Created",
                FromValue = "-",
                ToValue = "-",
                UserName = userName

            });

            await base.UpdateJobSchedule(_schedulerFactory, jobEntity); 

            return jobEntity;

        }
    }
}
