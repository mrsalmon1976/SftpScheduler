using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;

namespace SftpScheduler.BLL.Commands.Job
{
    public interface ICreateJobCommand
    {
        Task<JobEntity> ExecuteAsync(IDbContext dbContext, JobEntity jobEntity);
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
        
        public virtual async Task<JobEntity> ExecuteAsync(IDbContext dbContext, JobEntity jobEntity)
        {
            base.ValidateAndPrepareJobEntity(dbContext, _jobValidator, jobEntity);

            string sql = @"INSERT INTO Job 
                (Name, HostId, Type, Schedule, ScheduleInWords, LocalPath, RemotePath, DeleteAfterDownload, RemoteArchivePath, LocalCopyPaths, IsEnabled, Created, RestartOnFailure, CompressionMode, FileMask, PreserveTimestamp, TransferMode) 
                VALUES 
                (@Name, @HostId, @Type, @Schedule, @ScheduleInWords, @LocalPath, @RemotePath, @DeleteAfterDownload, @RemoteArchivePath, @LocalCopyPaths, @IsEnabled, @Created, @RestartOnFailure, @CompressionMode, @FileMask, @PreserveTimestamp, @TransferMode)";
            await dbContext.ExecuteNonQueryAsync(sql, jobEntity);

            sql = @"select last_insert_rowid()";
            jobEntity.Id = await dbContext.ExecuteScalarAsync<int>(sql);

            await base.UpdateJobSchedule(_schedulerFactory, jobEntity); 

            return jobEntity;

        }
    }
}
