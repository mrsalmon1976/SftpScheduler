using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface ISessionWrapperFactory
    {
        ISessionWrapper CreateSession(HostEntity hostEntity, JobEntity jobEntity);
    }

    public class SessionWrapperFactory : ISessionWrapperFactory
    {
        private const int DefaultPort = 21;

        public ISessionWrapper CreateSession(HostEntity hostEntity, JobEntity jobEntity)
        {
            // Setup session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,   // TODO: could be Ftps
                HostName = hostEntity.Host,
                PortNumber = hostEntity.Port ?? DefaultPort,
                UserName = hostEntity.Username,
                Password = hostEntity.Password,
                SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny  // TODO!  This needs to be removed!

                //SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx..."
            };

            ISessionWrapper sessionWrapper = new SessionWrapper(sessionOptions);
            return sessionWrapper;
        }
    }
}
