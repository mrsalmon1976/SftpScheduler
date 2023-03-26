using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Models
{
    public class HostViewModel
    {
        private string _hashId = "";

        public int Id { get; set; }

        public string HashId
        {
            get
            {
                if (String.IsNullOrEmpty(_hashId) && this.Id > 0)
                {
                    _hashId = UrlUtils.Encode(this.Id);
                }
                return _hashId;
            }
        }

        public string? Name { get; set; }

        public string? Host { get; set; }

        public int Port { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public int JobCount { get; set; }
    }
}
