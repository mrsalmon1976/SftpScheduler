using SftpScheduler.BLL.Data;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Models.Host
{
    public class HostViewModel
    {

        private string _hashId = "";

        public HostViewModel()
        {
            KeyFingerprint = string.Empty;
        }

        public int Id { get; set; }

        public string HashId
        {
            get
            {
                if (string.IsNullOrEmpty(_hashId) && Id > 0)
                {
                    _hashId = UrlUtils.Encode(Id);
                }
                return _hashId;
            }
        }

        public string? Name { get; set; }

        public string? Host { get; set; }

        public int Port { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public string KeyFingerprint { get; set; }

        public TransferProtocol Protocol { get; set; }

        public string ProtocolName {  get
            {
                return this.Protocol.ToString().ToUpper();
            } 
        }

        public FtpsMode FtpsMode { get; set; }

        public int JobCount { get; set; }
    }
}
