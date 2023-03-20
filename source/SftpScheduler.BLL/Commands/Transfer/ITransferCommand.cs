using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface ITransferCommand : IDisposable
    {
        void Execute(int jobId);
    }
}
