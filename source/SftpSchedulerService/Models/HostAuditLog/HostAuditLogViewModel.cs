using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Models.HostAuditLog
{
    public class HostAuditLogViewModel
    {

        private string _hashId = "";

        public HostAuditLogViewModel()
        {
            this.PropertyName = String.Empty;
            this.FromValue = String.Empty;
            this.ToValue = String.Empty;
            this.UserName = String.Empty;
        }

        public int Id { get; set; }

        public int HostId { get; set; }

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

        public string PropertyName { get; set; }

        public string FromValue { get; set; }

        public string ToValue { get; set; }

        public virtual string UserName { get; set; }

        public DateTime Created { get; set; }
    }
}
