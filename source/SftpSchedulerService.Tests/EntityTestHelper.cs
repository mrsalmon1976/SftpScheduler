using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests
{
    internal class EntityTestHelper
    {

        internal static HostEntity CreateHostEntity(int hostId = 0)
        {
            HostEntity hostEntity = new HostEntity();
            hostEntity.Id = hostId;
            hostEntity.Name = Faker.Lorem.GetFirstWord();
            hostEntity.Host = Faker.Internet.DomainName();
            hostEntity.Port = Faker.RandomNumber.Next(1, 9999);
            hostEntity.Username = Faker.Name.First();
            hostEntity.Password = Guid.NewGuid().ToString();
            return hostEntity;

        }

        internal static HostJobCountEntity CreateHostJobCountEntity(int hostId)
        {
            HostJobCountEntity hostJobCountEntity = new HostJobCountEntity();
            hostJobCountEntity.HostId = hostId;
            hostJobCountEntity.JobCount = Faker.RandomNumber.Next(0, 20);
            return hostJobCountEntity;
        }

        internal static JobEntity CreateJobEntity()
        {
            JobEntity jobEntity = new JobEntity();
            jobEntity.Name = Faker.Lorem.GetFirstWord();
            jobEntity.HostId = Faker.RandomNumber.Next(1, 9999);
            return jobEntity;

        }

        internal static JobLogEntity CreateJobLogEntity(int jobId)
        {
            const int Days100 = 8640000;

            JobLogEntity jobLogEntity = new JobLogEntity();
            jobLogEntity.JobId = jobId;
            jobLogEntity.StartDate = DateTime.Now.AddSeconds(Faker.RandomNumber.Next(1, Days100) * -1);
            jobLogEntity.Progress = Faker.RandomNumber.Next(1, 100);
            jobLogEntity.Status = JobStatus.InProgress;
            return jobLogEntity;

        }

    }
}
