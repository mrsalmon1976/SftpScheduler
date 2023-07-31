using Humanizer;
using SftpScheduler.BLL.Data;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Models.Job
{
    public class JobFileLogViewModel
    {
        private string _hashId = "";

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

        public string FileName { get; set; } = String.Empty;

        public long FileLength { get; set; }

        public string FileLengthReadable
        {
            get
            {
                if (this.FileLength <= 0)
                {
                    return "0 B";
                }

                var kb = this.FileLength.Bytes();
                return kb.Humanize();
            }
        }

    }
}
