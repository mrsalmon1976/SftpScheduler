using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.BootStrapping;
using SftpSchedulerService.Caching;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Models.Notification;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Notification
{
    [TestFixture]
    public class JobNotificationFetchAllOrchestratorTests
    {
        [Test]
        public void Execute_NoNotifcations_ReturnsEmptyList()
        {
            // setup
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            JobRepository jobRepo = Substitute.For<JobRepository>();
            
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);

            jobRepo.GetAllFailingAsync(dbContext).Returns(Enumerable.Empty<JobEntity>());
            jobRepo.GetAllFailedSinceAsync(dbContext, Arg.Any<DateTime>()).Returns(Enumerable.Empty<JobEntity>());

            // execute
            JobNotificationFetchAllOrchestrator orchestrator = CreateOrchestrator(dbContextFactory, Substitute.For<ICacheProvider>(), jobRepo);
            var result = orchestrator.Execute(false).Result as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);

            IEnumerable<JobNotificationViewModel> notificatins = result.Value as IEnumerable<JobNotificationViewModel>;
            Assert.That(notificatins, Is.Not.Null);
            Assert.That(notificatins.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Execute_FailuresExist_ReturnsList()
        {
            // setup
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);

            List<JobEntity> failingJobs = new List<JobEntity>();
            int failingJobCount = Faker.RandomNumber.Next(2, 7);
            for (int i=1; i<=failingJobCount; i++)
            {
                failingJobs.Add(EntityTestHelper.CreateJobEntity(i));
            }

            jobRepo.GetAllFailingAsync(dbContext).Returns(failingJobs);
            jobRepo.GetAllFailedSinceAsync(dbContext, Arg.Any<DateTime>()).Returns(Enumerable.Empty<JobEntity>());

            // execute
            JobNotificationFetchAllOrchestrator orchestrator = CreateOrchestrator(dbContextFactory, Substitute.For<ICacheProvider>(), jobRepo);
            var result = orchestrator.Execute(false).Result as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);

            IEnumerable<JobNotificationViewModel> notifications = result.Value as IEnumerable<JobNotificationViewModel>;
            Assert.That(notifications, Is.Not.Null);
            Assert.That(notifications.Count(), Is.EqualTo(failingJobCount));
        }

        [Test]
        public void Execute_FailuresInCache_ReturnsListFromCache()
        {
            // setup
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            ICacheProvider cacheProvider = Substitute.For<ICacheProvider>();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);

            List<JobNotificationViewModel> notifications = new List<JobNotificationViewModel>();
            notifications.Add(new JobNotificationViewModel(1, Guid.NewGuid().ToString(), AppConstants.NotificationType.Error));
            cacheProvider.Get<List<JobNotificationViewModel>>(CacheKeys.JobNotifications).Returns(notifications);

            // execute
            JobNotificationFetchAllOrchestrator orchestrator = CreateOrchestrator(dbContextFactory, cacheProvider, jobRepo);
            var result = orchestrator.Execute(false).Result as OkObjectResult;

            // assert
            cacheProvider.Received(1).Get<List<JobNotificationViewModel>>(CacheKeys.JobNotifications);
            jobRepo.DidNotReceive().GetAllFailingAsync(Arg.Any<IDbContext>()).GetAwaiter().GetResult();
            jobRepo.DidNotReceive().GetAllFailedSinceAsync(Arg.Any<IDbContext>(), Arg.Any<DateTime>()).GetAwaiter().GetResult();

            IEnumerable<JobNotificationViewModel> notificationsResult = result.Value as IEnumerable<JobNotificationViewModel>;
            Assert.That(notificationsResult, Is.Not.Null);
            Assert.That(notificationsResult.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Execute_FailuresInCacheButForceReload_ReturnsListFromDatabase()
        {
            // setup
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            ICacheProvider cacheProvider = Substitute.For<ICacheProvider>();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);

            List<JobNotificationViewModel> notifications = new List<JobNotificationViewModel>();
            notifications.Add(new JobNotificationViewModel(1, Guid.NewGuid().ToString(), AppConstants.NotificationType.Error));
            cacheProvider.Get<List<JobNotificationViewModel>>(CacheKeys.JobNotifications).Returns(notifications);

            // execute
            JobNotificationFetchAllOrchestrator orchestrator = CreateOrchestrator(dbContextFactory, cacheProvider, jobRepo);
            var result = orchestrator.Execute(true).Result as OkObjectResult;

            // assert
            cacheProvider.Received(1).Get<List<JobNotificationViewModel>>(CacheKeys.JobNotifications);
            jobRepo.Received(1).GetAllFailingAsync(Arg.Any<IDbContext>()).GetAwaiter().GetResult();
            jobRepo.Received(1).GetAllFailedSinceAsync(Arg.Any<IDbContext>(), Arg.Any<DateTime>()).GetAwaiter().GetResult();
            cacheProvider.Received(1).Set(CacheKeys.JobNotifications, Arg.Any<object>(), Arg.Any<TimeSpan>());
            }


        [Test]
        public void Execute_WarningsExist_ReturnsList()
        {
            // setup
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);

            List<JobEntity> warningJobs = new List<JobEntity>();
            int warningJobCount = Faker.RandomNumber.Next(2, 7);
            for (int i = 1; i <= warningJobCount; i++)
            {
                warningJobs.Add(EntityTestHelper.CreateJobEntity(i));
            }

            jobRepo.GetAllFailingAsync(dbContext).Returns(Enumerable.Empty<JobEntity>());
            jobRepo.GetAllFailedSinceAsync(dbContext, Arg.Any<DateTime>()).Returns(warningJobs);

            // execute
            JobNotificationFetchAllOrchestrator orchestrator = CreateOrchestrator(dbContextFactory, Substitute.For<ICacheProvider>(), jobRepo);
            var result = orchestrator.Execute(false).Result as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);

            IEnumerable<JobNotificationViewModel> notifications = result.Value as IEnumerable<JobNotificationViewModel>;
            Assert.That(notifications, Is.Not.Null);
            Assert.That(notifications.Count(), Is.EqualTo(warningJobCount));
        }

        [Test]
        public void Execute_ErrorsAndWarningsExist_ReturnsList()
        {
            // setup
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);

            List<JobEntity> warningJobs = new List<JobEntity>();
            int warningJobCount = Faker.RandomNumber.Next(2, 7);
            for (int i = 1; i <= warningJobCount; i++)
            {
                warningJobs.Add(EntityTestHelper.CreateJobEntity(i));
            }

            List<JobEntity> failingJobs = new List<JobEntity>();
            int failingJobCount = Faker.RandomNumber.Next(2, 7);
            for (int i = 1; i <= failingJobCount; i++)
            {
                failingJobs.Add(EntityTestHelper.CreateJobEntity(i + 100));
            }

            int totalJobCount = failingJobCount + warningJobCount;

            jobRepo.GetAllFailingAsync(dbContext).Returns(failingJobs);
            jobRepo.GetAllFailedSinceAsync(dbContext, Arg.Any<DateTime>()).Returns(warningJobs);

            // execute
            JobNotificationFetchAllOrchestrator orchestrator = CreateOrchestrator(dbContextFactory, Substitute.For<ICacheProvider>(), jobRepo);
            var result = orchestrator.Execute(false).Result as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);

            IEnumerable<JobNotificationViewModel> notifications = result.Value as IEnumerable<JobNotificationViewModel>;
            Assert.That(notifications, Is.Not.Null);
            Assert.That(notifications.Count(), Is.EqualTo(totalJobCount));
        }

        [Test]
        public void Execute_ErrorsAndWarningsIntersect_ReturnsListWithoutIntersections()
        {
            // setup
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);

            List<JobEntity> failingJobs = new List<JobEntity>();
            failingJobs.Add(EntityTestHelper.CreateJobEntity(1));
            failingJobs.Add(EntityTestHelper.CreateJobEntity(2));

            List<JobEntity> warningJobs = new List<JobEntity>();
            warningJobs.Add(EntityTestHelper.CreateJobEntity(2));
            warningJobs.Add(EntityTestHelper.CreateJobEntity(3));

            jobRepo.GetAllFailingAsync(dbContext).Returns(failingJobs);
            jobRepo.GetAllFailedSinceAsync(dbContext, Arg.Any<DateTime>()).Returns(warningJobs);

            // execute
            JobNotificationFetchAllOrchestrator orchestrator = CreateOrchestrator(dbContextFactory, Substitute.For<ICacheProvider>(), jobRepo);
            var result = orchestrator.Execute(false).Result as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);

            IEnumerable<JobNotificationViewModel> notifications = result.Value as IEnumerable<JobNotificationViewModel>;
            Assert.That(notifications, Is.Not.Null);
            Assert.That(notifications.Count(), Is.EqualTo(3));

            Assert.That(notifications.SingleOrDefault(x => x.JobId == 1 && x.NotificationType == AppConstants.NotificationType.Error), Is.Not.Null);
            Assert.That(notifications.SingleOrDefault(x => x.JobId == 2 && x.NotificationType == AppConstants.NotificationType.Error), Is.Not.Null);
                                            
            Assert.That(notifications.SingleOrDefault(x => x.JobId == 2 && x.NotificationType == AppConstants.NotificationType.Warning), Is.Null);
            Assert.That(notifications.SingleOrDefault(x => x.JobId == 3 && x.NotificationType == AppConstants.NotificationType.Warning), Is.Not.Null);

        }

        private JobNotificationFetchAllOrchestrator CreateOrchestrator(IDbContextFactory dbContextFactory, ICacheProvider cacheProvider, JobRepository jobRepo)
        {
            return new JobNotificationFetchAllOrchestrator(dbContextFactory, cacheProvider, jobRepo);
        }

    }
}
