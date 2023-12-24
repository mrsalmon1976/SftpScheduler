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
                Protocol = MapProtocol(hostEntity),
                HostName = hostEntity.Host,
                PortNumber = hostEntity.Port ?? DefaultPort,
                UserName = hostEntity.Username
            };

            if (!String.IsNullOrWhiteSpace(hostEntity.Password))
            {
                sessionOptions.Password = DecryptPassword(hostEntity);
            }


            if (hostEntity.Protocol == Data.TransferProtocol.Ftps)
            {
                sessionOptions.FtpSecure = MapFtpsMode(hostEntity);
            }
            else if (hostEntity.Protocol == Data.TransferProtocol.Sftp)
            {
                if (String.IsNullOrEmpty(hostEntity.KeyFingerprint))
                {
                    sessionOptions.SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny;
                }
                else
                {
                    sessionOptions.SshHostKeyFingerprint = hostEntity.KeyFingerprint;
                }
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

        private static FtpSecure MapFtpsMode(HostEntity host)
        {
            if (host.FtpsMode == Data.FtpsMode.None)
            {
                return FtpSecure.None;
            }
            else if (host.FtpsMode == Data.FtpsMode.Explicit) 
            {
                return FtpSecure.Explicit;
            }
            else if (host.FtpsMode == Data.FtpsMode.Implicit)
            {
                return FtpSecure.Implicit;
            }
            throw new NotSupportedException($"Unsupported FTPS mode {host.FtpsMode}");
        }

        private static Protocol MapProtocol(HostEntity host)
        {
            if (host.Protocol == Data.TransferProtocol.Sftp)
            {
                return Protocol.Sftp;
            }
            else if (host.Protocol == Data.TransferProtocol.Ftp)
            {
                return Protocol.Ftp;
            }
            else if (host.Protocol == Data.TransferProtocol.Ftps)
            {
                return Protocol.Ftp;
            }
            throw new NotSupportedException($"Unsupported protocol {host.Protocol}");
        }

    }
}
