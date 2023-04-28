using AutoMapper;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Host;
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
            cfg.CreateMap<JobLogEntity, JobLogViewModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToLocalTime()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate == null ? (DateTime?)null : src.EndDate.Value.ToLocalTime()));
            cfg.CreateMap<JobLogViewModel, JobLogEntity>();
        }

    }
}
