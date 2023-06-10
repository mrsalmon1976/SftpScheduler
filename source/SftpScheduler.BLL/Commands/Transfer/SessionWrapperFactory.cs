using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Security;
using WinSCP;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface ISessionWrapperFactory
    {
        ISessionWrapper CreateSession(HostEntity hostEntity);
    }

    public class SessionWrapperFactory : ISessionWrapperFactory
    {
        private const int DefaultPort = 21;
        private readonly IEncryptionProvider _encryptionProvider;

        public SessionWrapperFactory(IEncryptionProvider encryptionProvider)
        {
            _encryptionProvider = encryptionProvider;
        }

        public ISessionWrapper CreateSession(HostEntity hostEntity)
        {

            // Setup session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,   // TODO: could be Ftps
                HostName = hostEntity.Host,
                PortNumber = hostEntity.Port ?? DefaultPort,
                UserName = hostEntity.Username
                //ssh
                //SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny  // TODO!  This needs to be removed!

                //SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx..."
            };

            if (!String.IsNullOrWhiteSpace(hostEntity.Password))
            {
                sessionOptions.Password = DecryptPassword(hostEntity);
            }

            if (String.IsNullOrEmpty(hostEntity.KeyFingerprint))
            {
                sessionOptions.SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny;
            }
            else
            {
                sessionOptions.SshHostKeyFingerprint = hostEntity.KeyFingerprint;
            }

            ISessionWrapper sessionWrapper = new SessionWrapper(sessionOptions);
            return sessionWrapper;
        }

        private string DecryptPassword(HostEntity host)
        {
            try
            {
                return _encryptionProvider.Decrypt(host.Password);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Unable to decrypt password on host [{host.Name}]- this will need to be reset on the console.", ex);
            }

        }

    }
}
