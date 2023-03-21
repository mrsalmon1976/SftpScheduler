using Microsoft.Extensions.Logging;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface ITransferCommandFactory
    {
        ITransferCommand CreateTransferCommand();
    }

    public class TransferCommandFactory : ITransferCommandFactory
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ILoggerFactory _loggerFactory;

        public TransferCommandFactory(IDbContextFactory dbContextFactory, ILoggerFactory loggerFactory)
        {
            _dbContextFactory = dbContextFactory;
            _loggerFactory = loggerFactory;
        }

        public ITransferCommand CreateTransferCommand()
        {
            return new SftpTransferCommand(_loggerFactory.CreateLogger<SftpTransferCommand>(), _dbContextFactory, new JobRepository(), new HostRepository());
        }
    }
}
