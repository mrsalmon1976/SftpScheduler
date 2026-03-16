using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Host;

namespace SftpSchedulerService.Mapping
{
    public static class HostMappingExtensions
    {
        public static HostViewModel ToViewModel(this HostEntity hostEntity)
        {
            return new HostViewModel
            {
                Id = hostEntity.Id,
                Name = hostEntity.Name,
                Host = hostEntity.Host,
                Port = hostEntity.Port ?? 0,
                UserName = hostEntity.Username,
                Password = hostEntity.Password,
                KeyFingerprint = hostEntity.KeyFingerprint,
                Protocol = hostEntity.Protocol,
                FtpsMode = hostEntity.FtpsMode,
            };
        }

        public static HostEntity ToEntity(this HostViewModel hostViewModel)
        {
            return new HostEntity
            {
                Id = hostViewModel.Id,
                Name = hostViewModel.Name ?? string.Empty,
                Host = hostViewModel.Host ?? string.Empty,
                Port = hostViewModel.Port,
                Username = hostViewModel.UserName,
                Password = hostViewModel.Password ?? string.Empty,
                KeyFingerprint = hostViewModel.KeyFingerprint,
                Protocol = hostViewModel.Protocol,
                FtpsMode = hostViewModel.FtpsMode,
            };
        }
    }
}
