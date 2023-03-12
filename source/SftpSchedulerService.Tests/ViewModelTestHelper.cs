﻿using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Job;
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

        internal static JobViewModel CreateJobViewModel()
        {
            JobViewModel jobViewModel = new JobViewModel();
            jobViewModel.Name = Faker.Lorem.GetFirstWord();
            jobViewModel.HostId = Faker.RandomNumber.Next(1, 1000);
            return jobViewModel;

        }

    }
}