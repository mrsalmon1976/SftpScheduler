using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SftpScheduler.BLL.Commands.Job
{
    public interface IUpdateJobLogCompleteCommand
    {
        Task<JobLogEntity> ExecuteAsync(IDbContext dbContext, int jobLogId, int progress, string status, string? errorMessage);

    }

    public class UpdateJobLogCompleteCommand : IUpdateJobLogCompleteCommand
    {
        public virtual async Task<JobLogEntity> ExecuteAsync(IDbContext dbContext, int jobLogId, int progress, string status, string? errorMessage)
        {
            JobLogEntity jobLog = new JobLogEntity();
            jobLog.Id = jobLogId;
            jobLog.Progress = 100;
            jobLog.Status = status;
            jobLog.ErrorMessage = errorMessage;
            string sql = @"UPDATE JobLog SET EndDate = DATETIME(), Progress = @Progress, Status = @Status, ErrorMessage = @ErrorMessage WHERE Id = @Id";
            // , Progress = @Progress, Status = @Status, ErrorMessage = @ErrrorMessage 
            await dbContext.ExecuteNonQueryAsync(sql, jobLog);

            return jobLog;

        }

    }
}
