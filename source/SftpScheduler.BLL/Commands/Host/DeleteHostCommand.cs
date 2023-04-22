using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Repositories;
using System;

namespace SftpScheduler.BLL.Commands.Host
{
    public interface IDeleteHostCommand
    {
        Task<bool> ExecuteAsync(IDbContext dbContext, int hostId);
    }

    public class DeleteHostCommand : IDeleteHostCommand
    {
        private readonly HostRepository _hostRepo;

        public DeleteHostCommand(HostRepository hostRepo) 
        {
            _hostRepo = hostRepo;
        }

        public virtual async Task<bool> ExecuteAsync(IDbContext dbContext, int hostId)
        {
            int jobCount = await _hostRepo.GetJobCountAsync(dbContext, hostId);
            if (jobCount > 0)
            {
                throw new InvalidOperationException($"Host cannot be deleted: {jobCount} jobs still attached");
            }

            const string sqlJob = @"DELETE FROM Host WHERE Id = @Id";
            await dbContext.ExecuteNonQueryAsync(sqlJob, new { Id = hostId });

            return true;

        }
    }
}
