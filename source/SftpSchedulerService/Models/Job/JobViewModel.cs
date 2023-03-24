using SftpScheduler.BLL.Data;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Models.Job
{
    public class JobViewModel
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

        public int HostId { get; set; }

        public JobType Type { get; set; }

        public string? Schedule { get; set; }

        public string? ScheduleInWords { get; set; }

        public string? LocalPath { get; set; }

        public string? RemotePath { get; set; }
    }
}
