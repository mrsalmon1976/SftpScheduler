using AutoMapper;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.BootStrapping
{
    public class AutoMapperBootstrapper
    {
        public static void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<HostEntity, HostViewModel>();
            cfg.CreateMap<HostViewModel, HostEntity>();
            cfg.CreateMap<JobEntity, JobViewModel>();
            cfg.CreateMap<JobViewModel, JobEntity>();
            cfg.CreateMap<JobLogEntity, JobLogViewModel>();
            cfg.CreateMap<JobLogViewModel, JobLogEntity>();
        }
    }
}
