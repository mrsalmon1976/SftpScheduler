using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Quartz;
using SftpScheduler.BLL.Config;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Net;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Utility;
using SftpScheduler.Test.Common;
using System.Net.Mail;

namespace SftpScheduler.BLL.Tests.Jobs
{
	[TestFixture]
	public class DigestJobTests
	{
		[Test]
		public void Execute_SmtpNotConfigured_JobExits()
		{
			// setup
			IUserRepository userRepo = new SubstituteBuilder<IUserRepository>().Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
				.WithProperty(x => x.SmtpHost, String.Empty)
				.Build();

			// execute
			DigestJob digestJob = CreateDigestJob(userRepo: userRepo, userSettingProvider: globalUserSettingProvider);
			digestJob.Execute(Substitute.For<IJobExecutionContext>()).GetAwaiter().GetResult();

			// assert
			userRepo.DidNotReceive().GetUsersInRoleAsync(Arg.Any<string>());

		}

		[Test]
		public void Execute_NoFailingJobs_NoEmailSent()
		{
			// setup
			IEnumerable<JobEntity> failingJobs = Enumerable.Empty<JobEntity>();

            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));

            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();

			// execute
			DigestJob digestJob = CreateDigestJob(dbContextFactory, jobRepo: jobRepo, smtpClient: smtpClient);
			digestJob.Execute(Substitute.For<IJobExecutionContext>()).GetAwaiter().GetResult();

			// assert
			smtpClient.DidNotReceive().Send(Arg.Any<SmtpHost>(), Arg.Any<MailMessage>());

		}


		[Test]
		public void Execute_FailingJobs_EmailSent()
		{
			// setup
			IEnumerable<JobEntity> failingJobs = new List<JobEntity>
			{
				new SubstituteBuilder < JobEntity >().WithRandomProperties().Build()
			};

			List<UserEntity> adminUsers = new List<UserEntity>
			{
                new SubstituteBuilder<UserEntity>().WithRandomProperties().WithProperty(x => x.Email, "test@sftpscheduler.test").Build()
            };

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();

			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));

            IUserRepository userRepo = new SubstituteBuilder<IUserRepository>().Build();
            userRepo.GetUsersInRoleAsync(UserRoles.Admin).Returns(adminUsers);
            ResourceUtils resourceUtils = new SubstituteBuilder<ResourceUtils>().Build();
			resourceUtils.ReadResource(ResourceKey.DigestEmailTemplate).Returns(Faker.Lorem.Paragraph());
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpHost, "mailhost")
                .Build();
            globalUserSettingProvider.BuildDefaultMailMessage().Returns(mailMessage);
            globalUserSettingProvider.BuildSmtpHostFromSettings().Returns(smtpHost);

            // execute
            DigestJob digestJob = CreateDigestJob(dbContextFactory, resourceUtils, jobRepo, userRepo, smtpClient, globalUserSettingProvider);
			digestJob.Execute(Substitute.For<IJobExecutionContext>()).GetAwaiter().GetResult();

			// assert
			resourceUtils.Received(1).ReadResource(ResourceKey.DigestEmailTemplate);
			smtpClient.Received(1).Send(smtpHost, mailMessage);
		}

		[Test]
		public void Execute_FailingJobs_ValidEmailSent()
		{
			// setup
			IEnumerable<JobEntity> failingJobs = new List<JobEntity>
			{
				new SubstituteBuilder < JobEntity >().WithRandomProperties().Build()
			};

			List<UserEntity> adminUsers = new List<UserEntity>
			{
                new SubstituteBuilder<UserEntity>().WithRandomProperties().WithProperty(x => x.Email, "test@sftpscheduler.test").Build()
            };

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();
			string emailBody = Faker.Lorem.Paragraph();

			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));
            IUserRepository userRepo = new SubstituteBuilder<IUserRepository>().Build();
			userRepo.GetUsersInRoleAsync(UserRoles.Admin).Returns(adminUsers);
            ResourceUtils resourceUtils = new SubstituteBuilder<ResourceUtils>().Build();
            resourceUtils.ReadResource(ResourceKey.DigestEmailTemplate).Returns(emailBody);
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();

			IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
				.WithProperty(x => x.SmtpHost, "mailhost")
				.Build();
			globalUserSettingProvider.BuildDefaultMailMessage().Returns(mailMessage);
            globalUserSettingProvider.BuildSmtpHostFromSettings().Returns(smtpHost);

			// execute
			DigestJob digestJob = CreateDigestJob(dbContextFactory, resourceUtils, jobRepo, userRepo, smtpClient, globalUserSettingProvider);
			digestJob.Execute(Substitute.For<IJobExecutionContext>()).GetAwaiter().GetResult();

			// assert
			Assert.That(mailMessage.To.Single().Address, Is.EqualTo(adminUsers.Single().Email));
			Assert.That(mailMessage.Body, Is.EqualTo(emailBody));

			string expectedSubject = $"{DigestJob.DigestJobEmailSubject} : {DateTime.Now.ToString("yyyy-MM-dd")}";
			Assert.That(mailMessage.Subject, Is.EqualTo(expectedSubject));

		}

        [Test]
        public void Execute_FailingJobs_DisabledAdministratorsNotEmailed()
        {
			// setup
			IEnumerable<JobEntity> failingJobs = new[] { new SubstituteBuilder<JobEntity>().WithRandomProperties().Build() };

            int adminCount = Faker.RandomNumber.Next(2, 10);
            List<UserEntity> adminUsers = new List<UserEntity>();
			DateTimeOffset? lockoutEnd = DateTimeOffset.MaxValue;
            for (int i = 0; i < adminCount; i++)
            {
				UserEntity user = new SubstituteBuilder<UserEntity>().WithRandomProperties().WithProperty(x => x.LockoutEnd, lockoutEnd).Build();
                adminUsers.Add(user);
            };

            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();
            IUserRepository userRepo = new SubstituteBuilder<IUserRepository>().Build();
            userRepo.GetUsersInRoleAsync(UserRoles.Admin).Returns(adminUsers);

            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpHost, "mailhost")
                .Build();

			// execute
			DigestJob digestJob = CreateDigestJob(dbContextFactory, jobRepo: jobRepo, userRepo: userRepo, smtpClient: smtpClient, userSettingProvider: globalUserSettingProvider);
            digestJob.Execute(Substitute.For<IJobExecutionContext>()).GetAwaiter().GetResult();

            // assert
            userRepo.Received(1).GetUsersInRoleAsync(UserRoles.Admin);
            smtpClient.DidNotReceive().Send(Arg.Any<SmtpHost>(), Arg.Any<MailMessage>());
        }

        [Test]
		public void Execute_FailingJobs_AllEnabledAdministratorsEmailed()
		{
			// setup
			IEnumerable<JobEntity> failingJobs = new List<JobEntity>
			{
				new SubstituteBuilder < JobEntity >().WithRandomProperties().Build()
			};

			int adminCount = Faker.RandomNumber.Next(2, 10);
			List<UserEntity> adminUsers = new List<UserEntity>();
            DateTimeOffset? lockoutEnd = null;
            for (int i=0; i< adminCount; i++) 
			{
                UserEntity user = new SubstituteBuilder<UserEntity>().WithRandomProperties().WithProperty(x => x.Email, $"test{i}@sftpscheduler.test").WithProperty(x => x.LockoutEnd, lockoutEnd).Build();
                adminUsers.Add(user);
			};

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();
			string emailBody = Faker.Lorem.Paragraph();

			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));
            IUserRepository userRepo = new SubstituteBuilder<IUserRepository>().Build();
            userRepo.GetUsersInRoleAsync(UserRoles.Admin).Returns(adminUsers);
            ResourceUtils resourceUtils = new SubstituteBuilder<ResourceUtils>().Build();
            resourceUtils.ReadResource(ResourceKey.DigestEmailTemplate).Returns(emailBody);

            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();

            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpHost, "mailhost")
                .Build();
            globalUserSettingProvider.BuildDefaultMailMessage().Returns(mailMessage);
            globalUserSettingProvider.BuildSmtpHostFromSettings().Returns(smtpHost);

			// execute
			DigestJob digestJob = CreateDigestJob(dbContextFactory, resourceUtils, jobRepo, userRepo, smtpClient, globalUserSettingProvider);
			digestJob.Execute(Substitute.For<IJobExecutionContext>()).GetAwaiter().GetResult();

			// assert
			foreach (var user in adminUsers)
			{
				var toAddress = mailMessage.To.SingleOrDefault(x => x.Address == user.Email);
				Assert.That(toAddress, Is.Not.Null, $"Email {user.Email} not found in to collection list"); 
			}
		}

		[Test]
		public void Execute_FailedJobsInLast24Hours_EmailSentWithDetails()
		{
			// setup
			IEnumerable<JobEntity> failingJobs = new List<JobEntity>
			{
				new SubstituteBuilder < JobEntity >().WithRandomProperties().Build()
			};

			List<JobEntity> recentlyFailedJobs = new List<JobEntity>
			{
				new SubstituteBuilder < JobEntity >().WithRandomProperties().Build(),
				new SubstituteBuilder < JobEntity >().WithRandomProperties().Build()
			};

			List<UserEntity> adminUsers = new List<UserEntity>
			{
                new SubstituteBuilder<UserEntity>().WithRandomProperties().WithProperty(x => x.Email, "test@sftpscheduler.test").Build()
            };

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();
			string emailBody = DigestJob.RecentlyFailedJobsTag;

			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));
            jobRepo.GetAllFailedSinceAsync(dbContext, Arg.Any<DateTime>()).Returns(Task.FromResult((IEnumerable<JobEntity>)recentlyFailedJobs));

            IUserRepository userRepo = new SubstituteBuilder<IUserRepository>().Build();
            userRepo.GetUsersInRoleAsync(UserRoles.Admin).Returns(adminUsers);
            ResourceUtils resourceUtils = new SubstituteBuilder<ResourceUtils>().Build();
            resourceUtils.ReadResource(ResourceKey.DigestEmailTemplate).Returns(emailBody);
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpHost, "mailhost")
                .Build();
            globalUserSettingProvider.BuildDefaultMailMessage().Returns(mailMessage);
            globalUserSettingProvider.BuildSmtpHostFromSettings().Returns(smtpHost);

            // execute
            DigestJob digestJob = CreateDigestJob(dbContextFactory, resourceUtils, jobRepo, userRepo, smtpClient, globalUserSettingProvider);
			digestJob.Execute(Substitute.For<IJobExecutionContext>()).GetAwaiter().GetResult();

			// assert
			Assert.That(mailMessage.Body.Contains(recentlyFailedJobs[0].Name));
			Assert.That(mailMessage.Body.Contains(recentlyFailedJobs[1].Name));
		}

		[Test]
		public void Execute_NoFailedJobsInLast24Hours_EmailSentWithNoDetails()
		{
			// setup
			IEnumerable<JobEntity> failingJobs = new List<JobEntity>
			{
				new SubstituteBuilder<JobEntity>().WithRandomProperties().Build()
			};

			List<UserEntity> adminUsers = new List<UserEntity>
			{
				new SubstituteBuilder<UserEntity>().WithRandomProperties().WithProperty(x => x.Email, "test@sftpscheduler.test").Build()
            };

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();
			string emailBody = DigestJob.RecentlyFailedJobsTag;

			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IDbContextFactory dbContextFactory = new SubstituteBuilder<IDbContextFactory>().Build();
            dbContextFactory.GetDbContext().Returns(dbContext);
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));
            IUserRepository userRepo = new SubstituteBuilder<IUserRepository>().Build();
            userRepo.GetUsersInRoleAsync(UserRoles.Admin).Returns(adminUsers);
            ResourceUtils resourceUtils = new SubstituteBuilder<ResourceUtils>().Build();
            resourceUtils.ReadResource(ResourceKey.DigestEmailTemplate).Returns(emailBody);
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpHost, "mailhost")
                .Build();
            globalUserSettingProvider.BuildDefaultMailMessage().Returns(mailMessage);
            globalUserSettingProvider.BuildSmtpHostFromSettings().Returns(smtpHost);

            // execute
            DigestJob digestJob = CreateDigestJob(dbContextFactory, resourceUtils, jobRepo, userRepo, smtpClient, globalUserSettingProvider);
			digestJob.Execute(Substitute.For<IJobExecutionContext>()).GetAwaiter().GetResult();

			// assert
			Assert.That(mailMessage.Body, Is.EqualTo("<li>No jobs have failed recently</li>"));
		}

		private DigestJob CreateDigestJob(IDbContextFactory? dbContextFactory = null
			, ResourceUtils? resourceUtils = null
			, JobRepository? jobRepo = null
			, IUserRepository? userRepo = null
			, ISmtpClientWrapper? smtpClient = null
			, IGlobalUserSettingProvider? userSettingProvider = null)
		{
			ILogger<DigestJob> logger = Substitute.For<ILogger<DigestJob>>();
			dbContextFactory ??= Substitute.For<IDbContextFactory>();
			resourceUtils ??= Substitute.For<ResourceUtils>();
			jobRepo ??= Substitute.For<JobRepository>();
			userRepo ??= Substitute.For<IUserRepository>();
			smtpClient ??= Substitute.For<ISmtpClientWrapper>();
			userSettingProvider ??= Substitute.For<IGlobalUserSettingProvider>();

			return new DigestJob(logger, dbContextFactory, resourceUtils, jobRepo, userRepo, smtpClient, userSettingProvider);
		}


    }
}
