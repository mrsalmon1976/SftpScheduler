using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Job
{
    public interface IUpdateJobResultCompleteCommand
    {
        Task<JobResultEntity> ExecuteAsync(IDbContext dbContext, int jobResultId, int progress, string status, string? errorMessage);

    }

    public class UpdateJobResultCompleteCommand : IUpdateJobResultCompleteCommand
    {
        public virtual async Task<JobResultEntity> ExecuteAsync(IDbContext dbContext, int jobResultId, int progress, string status, string? errorMessage)
        {
            JobResultEntity jobResultEntity = new JobResultEntity();
            jobResultEntity.Id = jobResultId;
            jobResultEntity.Progress = 100;
            jobResultEntity.Status = status;
            jobResultEntity.ErrorMessage = errorMessage;
            string sql = @"UPDATE JobResult SET EndDate = DATETIME(), Progress = @Progress, Status = @Status, ErrorMessage = @ErrrorMessage WHERE Id = @Id)";
            await dbContext.ExecuteNonQueryAsync(sql, jobResultEntity);

            return jobResultEntity;

        }

    }
}
