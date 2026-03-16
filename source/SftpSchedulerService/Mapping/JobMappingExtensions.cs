using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.Mapping
{
    public static class JobMappingExtensions
    {
        public static JobViewModel ToViewModel(this JobEntity jobEntity)
        {
            return new JobViewModel
            {
                Id = jobEntity.Id,
                Name = jobEntity.Name,
                HostId = jobEntity.HostId,
                Type = jobEntity.Type,
                Schedule = jobEntity.Schedule,
                ScheduleInWords = jobEntity.ScheduleInWords,
                LocalPath = jobEntity.LocalPath,
                RemotePath = jobEntity.RemotePath,
                DeleteAfterDownload = jobEntity.DeleteAfterDownload,
                RemoteArchivePath = jobEntity.RemoteArchivePath,
                LocalArchivePath = jobEntity.LocalArchivePath,
                LocalPrefix = jobEntity.LocalPrefix,
                LocalCopyPaths = jobEntity.LocalCopyPaths,
                IsEnabled = jobEntity.IsEnabled,
                RestartOnFailure = jobEntity.RestartOnFailure,
                CompressionMode = jobEntity.CompressionMode,
                FileMask = jobEntity.FileMask,
                PreserveTimestamp = jobEntity.PreserveTimestamp,
                TransferMode = jobEntity.TransferMode,
            };
        }

        public static JobEntity ToEntity(this JobViewModel jobViewModel)
        {
            return new JobEntity
            {
                Id = jobViewModel.Id,
                Name = jobViewModel.Name ?? string.Empty,
                HostId = jobViewModel.HostId,
                Type = jobViewModel.Type,
                Schedule = jobViewModel.Schedule ?? string.Empty,
                LocalPath = jobViewModel.LocalPath ?? string.Empty,
                RemotePath = jobViewModel.RemotePath ?? string.Empty,
                DeleteAfterDownload = jobViewModel.DeleteAfterDownload,
                RemoteArchivePath = jobViewModel.RemoteArchivePath,
                LocalArchivePath = jobViewModel.LocalArchivePath,
                LocalPrefix = jobViewModel.LocalPrefix,
                LocalCopyPaths = jobViewModel.LocalCopyPaths,
                IsEnabled = jobViewModel.IsEnabled,
                RestartOnFailure = jobViewModel.RestartOnFailure,
                CompressionMode = jobViewModel.CompressionMode,
                FileMask = jobViewModel.FileMask,
                PreserveTimestamp = jobViewModel.PreserveTimestamp,
                TransferMode = jobViewModel.TransferMode,
            };
        }
    }
}
