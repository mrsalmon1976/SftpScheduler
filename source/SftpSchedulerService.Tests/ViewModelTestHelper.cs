using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SftpScheduler.BLL.Command.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;
using SftpSchedulerService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests
{
    internal class ViewModelTestHelper
    {

        internal static HostViewModel CreateHostViewModel()
        {
            HostViewModel hostEntity = new HostViewModel();
            hostEntity.Name = Faker.Lorem.GetFirstWord();
            hostEntity.Host = Faker.Internet.DomainName();
            hostEntity.Port = Faker.RandomNumber.Next(1, 9999);
            hostEntity.UserName = Faker.Name.First();
            hostEntity.Password = Guid.NewGuid().ToString();
            return hostEntity;

        }

        internal static JobViewModel CreateJobViewModel(int hostId)
        {
            JobViewModel jobViewModel = new JobViewModel();
            jobViewModel.Name = Faker.Lorem.GetFirstWord();
            jobViewModel.HostId = hostId;
            return jobViewModel;

        }

    }
}
