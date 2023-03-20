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
        public ITransferCommand CreateTransferCommand()
        {
            return new SftpTransferCommand();
        }
    }
}
