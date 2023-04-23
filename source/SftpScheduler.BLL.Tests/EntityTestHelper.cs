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

        internal static HostEntity CreateHostEntity(int id = 0)
        {
            HostEntity hostEntity = new HostEntity();
            hostEntity.Id = id;
            hostEntity.Name = Faker.Lorem.GetFirstWord();
            hostEntity.Host = Faker.Internet.DomainName();
            hostEntity.Port = Faker.RandomNumber.Next(1, 65535);
            hostEntity.Username = Faker.Name.First();
            hostEntity.Password = Guid.NewGuid().ToString();
            return hostEntity;

        }

        internal static JobEntity CreateJobEntity(int id = 0, int? hostId = null)
        {
            JobEntity jobEntity = new JobEntity();
            jobEntity.Id = id;
            jobEntity.Name = Faker.Lorem.GetFirstWord();
            jobEntity.HostId = hostId ?? Faker.RandomNumber.Next(1, 100);
            jobEntity.Type = BLL.Data.JobType.Download;
            jobEntity.Schedule = "0 * 0 ? * * *";
            jobEntity.ScheduleInWords = "Every minute";
            jobEntity.LocalPath = "\\\\localshare\\localfolder";
            jobEntity.RemotePath = "/remotefolder";
            jobEntity.DeleteAfterDownload = true;
            return jobEntity;

        }

        internal static JobLogEntity CreateJobLogEntity()
        {
            JobLogEntity jobLog = new JobLogEntity();
            jobLog.JobId = Faker.RandomNumber.Next(1, 100);
            jobLog.StartDate = DateTime.Now.AddSeconds(-5);
            return jobLog;

        }

    }
}
