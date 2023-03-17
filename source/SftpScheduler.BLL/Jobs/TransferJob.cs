using Quartz;
using SftpScheduler.BLL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Jobs
{
    public class TransferJob : IJob
    {
        public const string DefaultGroup = "TransferJob";

        private readonly IDbContextFactory _dbContextFactory;

        public TransferJob(IDbContextFactory dbContextFactory) 
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync($"Job executed [{context.JobDetail.Key}]");
        }
    }
}
