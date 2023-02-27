using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests
{
    internal class EntityTestHelper
    {

        internal static HostEntity CreateHostEntity()
        {
            HostEntity hostEntity = new HostEntity();
            hostEntity.Name = Faker.Lorem.GetFirstWord();
            hostEntity.Host = Faker.Internet.DomainName();
            hostEntity.Port = Faker.RandomNumber.Next(1, 9999);
            hostEntity.Username = Faker.Name.First();
            hostEntity.Password = Guid.NewGuid().ToString();
            return hostEntity;

        }

        internal static JobEntity CreateJobEntity(int hostId)
        {
            JobEntity jobEntity = new JobEntity();
            jobEntity.Name = Faker.Lorem.GetFirstWord();
            jobEntity.HostId = hostId;
            return jobEntity;

        }

    }
}
