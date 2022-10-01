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

        public CreateJobCommand()
        {
        }

        public JobEntity Execute(IDbContext dbContext, string name)
        {
            JobEntity job = new JobEntity();
            job.Name = name;

            string sql = @"INSERT INTO Jobs (Id, Name, Created) VALUES (@Id, @Name, @Created)";
            dbContext.ExecuteNonQuery(sql, job);

            return job;
        }
    }
}
