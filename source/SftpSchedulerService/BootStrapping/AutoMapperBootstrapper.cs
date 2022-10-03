using AutoMapper;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models;

namespace SftpSchedulerService.BootStrapping
{
    public class AutoMapperBootstrapper
    {
        public static void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<HostEntity, HostViewModel>();
            cfg.CreateMap<HostViewModel, HostEntity>();
        }
    }
}
