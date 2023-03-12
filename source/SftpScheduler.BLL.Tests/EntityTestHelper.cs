using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests
{
    internal class EntityTestHelper
    {

        internal static HostEntity CreateHostEntity()
        {
            HostEntity hostEntity = new HostEntity();
            hostEntity.Name = Faker.Lorem.GetFirstWord();
            hostEntity.Host = Faker.Internet.DomainName();
            hostEntity.Port = Faker.RandomNumber.Next(1, 65535);
            hostEntity.Username = Faker.Name.First();
            hostEntity.Password = Guid.NewGuid().ToString();
            return hostEntity;

        }

        internal static JobEntity CreateJobEntity()
        {
            JobEntity jobEntity = new JobEntity();
            jobEntity.Name = Faker.Lorem.GetFirstWord();
            jobEntity.HostId = Faker.RandomNumber.Next(1, 100);
            jobEntity.Schedule = "* * * * *";
            return jobEntity;

        }

    }
}
