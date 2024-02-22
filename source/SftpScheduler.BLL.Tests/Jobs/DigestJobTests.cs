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
using SftpScheduler.BLL.Tests.Builders.Config;
using SftpScheduler.BLL.Tests.Builders.Data;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Tests.Builders.Repositories;
using SftpScheduler.BLL.Tests.Builders.Utility;
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
			IUserRepository userRepo = new UserRepositoryBuilder().Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder().WithSmtpHost(String.Empty).Build();

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

			IDbContext dbContext = new DbContextBuilder().Build();
			IDbContextFactory dbContextFactory = new DbContextFactoryBuilder().WithDbContext(dbContext).Build();
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
				new JobEntityBuilder().WithRandomProperties().Build()
			};

			List<UserEntity> adminUsers = new List<UserEntity>
			{
				new UserEntityBuilder().WithRandomProperties().Build()
			};

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();

			IDbContext dbContext = new DbContextBuilder().Build();
			IDbContextFactory dbContextFactory = new DbContextFactoryBuilder().WithDbContext(dbContext).Build();
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));

			IUserRepository userRepo = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();
			ResourceUtils resourceUtils = new ResourceUtilsBuilder().WithReadResourceReturns(ResourceKey.DigestEmailTemplate, Faker.Lorem.Paragraph()).Build();
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();
            IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
				.WithSmtpHost("mailhost")
				.WithBuildDefaultMailMessageReturns(mailMessage)
				.WithBuildSmtpHostFromSettingsReturns(smtpHost)
				.Build();

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
				new JobEntityBuilder().WithRandomProperties().Build()
			};

			List<UserEntity> adminUsers = new List<UserEntity>
			{
				new UserEntityBuilder().WithRandomProperties().Build()
			};

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();
			string emailBody = Faker.Lorem.Paragraph();

			IDbContext dbContext = new DbContextBuilder().Build();
			IDbContextFactory dbContextFactory = new DbContextFactoryBuilder().WithDbContext(dbContext).Build();
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));
            IUserRepository userRepo = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();
			ResourceUtils resourceUtils = new ResourceUtilsBuilder().WithReadResourceReturns(ResourceKey.DigestEmailTemplate, emailBody).Build();
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();
            IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
				.WithSmtpHost("mailhost")
				.WithBuildDefaultMailMessageReturns(mailMessage)
				.WithBuildSmtpHostFromSettingsReturns(smtpHost)
				.Build();

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
			IEnumerable<JobEntity> failingJobs = new[] { new JobEntityBuilder().WithRandomProperties().Build() };

            int adminCount = Faker.RandomNumber.Next(2, 10);
            List<UserEntity> adminUsers = new List<UserEntity>();
            for (int i = 0; i < adminCount; i++)
            {
                adminUsers.Add(new UserEntityBuilder().WithRandomProperties().WithLockoutEnabled(true).Build());
            };

            IDbContext dbContext = new DbContextBuilder().Build();
            IDbContextFactory dbContextFactory = new DbContextFactoryBuilder().WithDbContext(dbContext).Build();
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();
            IUserRepository userRepo  = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();

            IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
                .WithSmtpHost("mailhost")
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
				new JobEntityBuilder().WithRandomProperties().Build()
			};

			int adminCount = Faker.RandomNumber.Next(2, 10);
			List<UserEntity> adminUsers = new List<UserEntity>();
			for (int i=0; i< adminCount; i++) {
				adminUsers.Add(new UserEntityBuilder().WithRandomProperties().WithLockoutEnabled(false).Build());
			};

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();
			string emailBody = Faker.Lorem.Paragraph();

			IDbContext dbContext = new DbContextBuilder().Build();
			IDbContextFactory dbContextFactory = new DbContextFactoryBuilder().WithDbContext(dbContext).Build();
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));
            IUserRepository userRepo = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();
			ResourceUtils resourceUtils = new ResourceUtilsBuilder().WithReadResourceReturns(ResourceKey.DigestEmailTemplate, emailBody).Build();
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();
            IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
				.WithSmtpHost("mailhost")
				.WithBuildDefaultMailMessageReturns(mailMessage)
				.WithBuildSmtpHostFromSettingsReturns(smtpHost)
				.Build();

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
				new JobEntityBuilder().WithRandomProperties().Build()
			};

			List<JobEntity> recentlyFailedJobs = new List<JobEntity>
			{
				new JobEntityBuilder().WithRandomProperties().Build(),
				new JobEntityBuilder().WithRandomProperties().Build()
			};

			List<UserEntity> adminUsers = new List<UserEntity>
			{
				new UserEntityBuilder().WithRandomProperties().Build()
			};

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();
			string emailBody = DigestJob.RecentlyFailedJobsTag;

			IDbContext dbContext = new DbContextBuilder().Build();
			IDbContextFactory dbContextFactory = new DbContextFactoryBuilder().WithDbContext(dbContext).Build();
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));
            jobRepo.GetAllFailedSinceAsync(dbContext, Arg.Any<DateTime>()).Returns(Task.FromResult((IEnumerable<JobEntity>)recentlyFailedJobs));

			IUserRepository userRepo = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();
			ResourceUtils resourceUtils = new ResourceUtilsBuilder().WithReadResourceReturns(ResourceKey.DigestEmailTemplate, emailBody).Build();
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();
            IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
				.WithSmtpHost("mailhost")
				.WithBuildDefaultMailMessageReturns(mailMessage)
				.WithBuildSmtpHostFromSettingsReturns(smtpHost)
				.Build();

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
				new JobEntityBuilder().WithRandomProperties().Build()
			};

			List<UserEntity> adminUsers = new List<UserEntity>
			{
				new UserEntityBuilder().WithRandomProperties().Build()
			};

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();
			string emailBody = DigestJob.RecentlyFailedJobsTag;

			IDbContext dbContext = new DbContextBuilder().Build();
			IDbContextFactory dbContextFactory = new DbContextFactoryBuilder().WithDbContext(dbContext).Build();
            JobRepository jobRepo = new SubstituteBuilder<JobRepository>().Build();
            jobRepo.GetAllFailingActiveAsync(dbContext).Returns(Task.FromResult(failingJobs));
			IUserRepository userRepo = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();
			ResourceUtils resourceUtils = new ResourceUtilsBuilder().WithReadResourceReturns(ResourceKey.DigestEmailTemplate, emailBody).Build();
            ISmtpClientWrapper smtpClient = new SubstituteBuilder<ISmtpClientWrapper>().Build();
            IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
				.WithSmtpHost("mailhost")
				.WithBuildDefaultMailMessageReturns(mailMessage)
				.WithBuildSmtpHostFromSettingsReturns(smtpHost)
				.Build();

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
