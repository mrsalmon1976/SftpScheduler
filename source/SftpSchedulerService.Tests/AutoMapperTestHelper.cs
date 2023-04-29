using AutoMapper;
using SftpSchedulerService.BootStrapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests
{
    internal class AutoMapperTestHelper
    {
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => AutoMapperBootstrapper.Configure(cfg));
            return config.CreateMapper();
        }
    }
}
