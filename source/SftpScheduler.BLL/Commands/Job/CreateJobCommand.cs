using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Command.Job
{
    public class CreateJobCommand
    {

        public async Task<JobViewModel> ExecuteAsync(IDbContext dbContext, string name, int hostId)
        {
            JobViewModel job = new JobViewModel();
            job.Name = name;
            job.HostId = hostId;

            string sql = @"INSERT INTO Job (Name, HostId, Created) VALUES (@Name, @HostId, @Created)";
            await dbContext.ExecuteNonQueryAsync(sql, job);

            return job;
        }
    }
}
