﻿using SftpScheduler.BLL.Data;
using SftpSchedulerService.Utilities;
using System.IO.Compression;

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

        public bool DeleteAfterDownload { get; set; }

        public virtual string? RemoteArchivePath { get; set; }

        public virtual string? LocalCopyPaths { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime? NextRunTime { get; set; }

        public bool RestartOnFailure { get; set; }

        public bool IsFailing { get; set; }

        public CompressionMode CompressionMode { get; set; }

        public string? FileMask { get; set; }

        public bool PreserveTimestamp { get; set; }

        public TransferMode TransferMode { get; set; }

        public string? LocalArchivePath { get; set; }

        public string? LocalPrefix { get; set; }


    }
}
