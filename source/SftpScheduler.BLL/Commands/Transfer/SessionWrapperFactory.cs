using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
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
        private readonly IPasswordProvider _passwordProvider;

        public SessionWrapperFactory(IPasswordProvider passwordProvider)
        {
            _passwordProvider = passwordProvider;
        }

        public ISessionWrapper CreateSession(HostEntity hostEntity, JobEntity jobEntity)
        {
            string password = _passwordProvider.Decrypt(hostEntity.Password);

            // Setup session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,   // TODO: could be Ftps
                HostName = hostEntity.Host,
                PortNumber = hostEntity.Port ?? DefaultPort,
                UserName = hostEntity.Username,
                Password = password,
                SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny  // TODO!  This needs to be removed!

                //SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx..."
            };

            ISessionWrapper sessionWrapper = new SessionWrapper(sessionOptions);
            return sessionWrapper;
        }
    }
}
