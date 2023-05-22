using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.Common.Diagnostics
{
    public interface IProcessWrapperFactory
    {
        IProcessWrapper CreateProcess();
    }

    public class ProcessWrapperFactory : IProcessWrapperFactory
    {
        public IProcessWrapper CreateProcess()
        {
            return new ProcessWrapper();
        }
    }
}
