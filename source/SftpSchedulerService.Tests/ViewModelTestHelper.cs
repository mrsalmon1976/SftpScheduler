using Microsoft.AspNetCore.Http;
using SftpScheduler.BLL.Data;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Models.Notification;
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

        internal static JobNotificationViewModel CreateJobNotificationViewModel(int jobId = 0, string? jobName = null, string notificationType = AppConstants.NotificationType.Information)
        {
            JobNotificationViewModel model = new JobNotificationViewModel();
            model.JobId = jobId;
            model.NotificationType = notificationType;
            model.JobName = (jobName ?? Guid.NewGuid().ToString());
            return model;
        }

        internal static JobViewModel CreateJobViewModel()
        {
            JobViewModel jobViewModel = new JobViewModel();
            jobViewModel.Name = Faker.Lorem.GetFirstWord();
            jobViewModel.HostId = Faker.RandomNumber.Next(1, 1000);
            return jobViewModel;

        }

        internal static JobLogViewModel CreateJobLogViewModel()
        {
            JobLogViewModel jobLogViewModel = new JobLogViewModel();
            jobLogViewModel.JobId = Faker.RandomNumber.Next(1, 1000);
            jobLogViewModel.StartDate = DateTime.Now.AddSeconds(Faker.RandomNumber.Next(1, Int32.MaxValue) * -1);
            jobLogViewModel.Progress = Faker.RandomNumber.Next(1, 100);
            jobLogViewModel.Status = Faker.Lorem.GetFirstWord();
            return jobLogViewModel;
        }

    }
}
