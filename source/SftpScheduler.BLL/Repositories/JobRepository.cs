using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Repositories
{
    public class JobRepository
    {
        public virtual async Task<IEnumerable<JobEntity>> GetAllAsync(IDbContext dbContext)
        {
            const string sql = @"SELECT * FROM Job ORDER BY Name";
            return await dbContext.QueryAsync<JobEntity>(sql);
        }

        public virtual async Task<int> GetAllCountAsync(IDbContext dbContext)
        {
            const string sql = @"SELECT COUNT(Id) FROM Job";
            return await dbContext.ExecuteScalarAsync<int>(sql);
        }

        public virtual async Task<JobEntity> GetByIdAsync(IDbContext dbContext, int id)
        {
            const string sql = @"SELECT * FROM Job WHERE Id = @Id";
            return await dbContext.QuerySingleAsync<JobEntity>(sql, new { Id = id });
        }

        public virtual async Task<JobEntity> GetByIdOrDefaultAsync(IDbContext dbContext, int id)
        {
            const string sql = @"SELECT * FROM Job WHERE Id = @Id";
            return await dbContext.QuerySingleOrDefaultAsync<JobEntity>(sql, new { Id = id });
        }

        public virtual async Task<IEnumerable<JobEntity>> GetAllFailingAsync(IDbContext dbContext)
        {
            const string sql = @"WITH LastJobLog(LastJobLogId, JobId) AS (
	                SELECT MAX(Id) AS LastJobLogId, JobId
	                FROM JobLog
                    WHERE Status <> @StatusInProgress
	                GROUP BY JobId
                )
                SELECT j.*
                from Job j 
                inner join LastJobLog ljl on j.Id = ljl.JobId
                inner join JobLog jl ON ljl.LastJobLogId = jl.Id
                WHERE jl.Status = @Status
                ORDER BY j.Name";
            return await dbContext.QueryAsync<JobEntity>(sql, new { StatusInProgress = JobStatus.InProgress, Status = JobStatus.Failed });

        }

        public virtual async Task<IEnumerable<JobEntity>> GetAllFailingActiveAsync(IDbContext dbContext)
        {
            const string sql = @"WITH LastJobLog(LastJobLogId, JobId) AS (
	                SELECT MAX(Id) AS LastJobLogId, JobId
	                FROM JobLog
                    WHERE Status <> @StatusInProgress
	                GROUP BY JobId
                )
                SELECT j.*
                from Job j 
                inner join LastJobLog ljl on j.Id = ljl.JobId
                inner join JobLog jl ON ljl.LastJobLogId = jl.Id
                WHERE jl.Status = @Status
                AND j.IsEnabled = 1
                ORDER BY j.Name";
            return await dbContext.QueryAsync<JobEntity>(sql, new { StatusInProgress = JobStatus.InProgress, Status = JobStatus.Failed });

        }

        public virtual async Task<IEnumerable<JobEntity>> GetAllFailedSinceAsync(IDbContext dbContext, DateTime dt)
        {
            string startDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
            const string sql = @"WITH FailedJobLog(JobId) AS (
	                                select DISTINCT JobId
	                                from JobLog 	
	                                WHERE StartDate >= @StartDate
	                                AND Status = @Status
                                )
                                SELECT j.*
                                from Job j 
                                inner join FailedJobLog fjl on j.Id = fjl.JobId
                                ORDER BY j.Name DESC";
            return await dbContext.QueryAsync<JobEntity>(sql, new { StartDate = startDate, Status = JobStatus.Failed });

        }

    }
}
