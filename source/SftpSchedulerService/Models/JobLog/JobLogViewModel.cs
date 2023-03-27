using SftpScheduler.BLL.Data;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Models.Job
{
    public class JobLogViewModel
    {
        private string _hashId = "";

        public JobLogViewModel()
        {

        }

        public int Id { get; set; }

        public int JobId { get; set; }

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

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int Progress { get; set; }

        public string? Status { get; set; }

        public string? ErrorMessage { get; set; }

    }
}
