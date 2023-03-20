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
    public interface ICreateJobResultCommand
    {
        Task<JobResultEntity> ExecuteAsync(IDbContext dbContext, int jobId);
    }

    public class CreateJobResultCommand : ICreateJobResultCommand
    {
        public virtual async Task<JobResultEntity> ExecuteAsync(IDbContext dbContext, int jobId)
        {
            JobResultEntity jobResultEntity = new JobResultEntity();
            jobResultEntity.JobId = jobId;
            jobResultEntity.StartDate = DateTime.Now;
            jobResultEntity.Status = JobResult.InProgress;
            string sql = @"INSERT INTO JobResult (JobId, StartDate, Progress, Status) VALUES (@JobId, DATETIME(), @Progress, @Status)";
            await dbContext.ExecuteNonQueryAsync(sql, jobResultEntity);

            sql = @"select last_insert_rowid()";
            jobResultEntity.Id = await dbContext.ExecuteScalarAsync<int>(sql);

            return jobResultEntity;

        }

    }
}
