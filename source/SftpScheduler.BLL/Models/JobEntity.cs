using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Models
{
    public class JobEntity
    {
        public JobEntity()
        {
            this.Id = 0;
            this.HostId = 0;
            this.Name = String.Empty;
            this.Schedule = String.Empty;
            this.RemotePath = String.Empty;
            this.ScheduleInWords = String.Empty;
            this.LocalPath = String.Empty;
            this.IsEnabled = true;
            this.Created = DateTime.UtcNow;
            this.RestartOnFailure = false;
            this.CompressionMode = CompressionMode.None;
            this.FileMask = String.Empty;
            this.PreserveTimestamp = true;
            this.TransferMode = TransferMode.Binary;
        }

        public virtual int Id { get; set; }

        [Required]
        public virtual string Name { get; set; }

        [Required]
        [Range(1, Int32.MaxValue)]
        public virtual int HostId { get; set; }

        [Required(ErrorMessage = "Job type is required")]
        [Range(1, 2, ErrorMessage = "Job type must be 1 (Download) or 2 (Upload)")]
        public virtual JobType Type { get; set; }

        [Required]
        [CronSchedule]
        public virtual string Schedule { get; set; }

        // not marked as required as this is a "cached" value and system-handled
        public virtual string ScheduleInWords { get; internal set; }

        [Required(ErrorMessage = "Local path is required")]  
        public virtual string LocalPath { get; set; }

        [Required(ErrorMessage = "Remote path is required")]
        public virtual string RemotePath { get; set; }

        public virtual bool DeleteAfterDownload { get; set; }

        public virtual string? RemoteArchivePath { get; set; }

        public virtual string? LocalCopyPaths { get; set; }

        public bool IsEnabled { get; set; }

        public virtual DateTime Created { get; internal set; }

        public bool RestartOnFailure { get; set; }

        public CompressionMode CompressionMode { get; set; }

        public string? FileMask { get; set; }

        public bool PreserveTimestamp { get; set; }

        public TransferMode TransferMode { get; set; }


        public virtual IEnumerable<string> LocalCopyPathsAsEnumerable()
        {
            if (String.IsNullOrWhiteSpace(this.LocalCopyPaths)) 
            { 
                return Enumerable.Empty<string>(); 
            }
            
            return this.LocalCopyPaths.Split(';').Where(x => !String.IsNullOrWhiteSpace(x));
        }
    }
}
