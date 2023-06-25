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
using SftpScheduler.BLL.TestInfrastructure.Config;
using SftpScheduler.BLL.TestInfrastructure.Data;
using SftpScheduler.BLL.TestInfrastructure.Models;
using SftpScheduler.BLL.TestInfrastructure.Net;
using SftpScheduler.BLL.TestInfrastructure.Repositories;
using SftpScheduler.BLL.TestInfrastructure.Utility;
using SftpScheduler.BLL.Utility;
using System.Net.Mail;

namespace SftpScheduler.BLL.Tests.Jobs
{
	[TestFixture]
	public class DigestJobTests
	{
		[Test]
		public void Execute_NoFailingJobs_NoEmailSent()
		{
			// setup
			IEnumerable<JobEntity> failingJobs = Enumerable.Empty<JobEntity>();

			IDbContext dbContext = new DbContextBuilder().Build();
			IDbContextFactory dbContextFactory = new DbContextFactoryBuilder().WithDbContext(dbContext).Build();
			JobRepository jobRepo = new JobRepositoryBuilder().WithGetAllFailingActiveAsyncReturns(dbContext, failingJobs).Build();
			ISmtpClientWrapper smtpClient = new SmtpClientWrapperBuilder().Build();

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
			List<JobEntity> failingJobs = new List<JobEntity>
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
			JobRepository jobRepo = new JobRepositoryBuilder().WithGetAllFailingActiveAsyncReturns(dbContext, failingJobs).Build();
			IUserRepository userRepo = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();
			ResourceUtils resourceUtils = new ResourceUtilsBuilder().WithReadResourceReturns(ResourceKey.DigestEmailTemplate, Faker.Lorem.Paragraph()).Build();
			ISmtpClientWrapper smtpClient = new SmtpClientWrapperBuilder().Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
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
			List<JobEntity> failingJobs = new List<JobEntity>
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
			JobRepository jobRepo = new JobRepositoryBuilder().WithGetAllFailingActiveAsyncReturns(dbContext, failingJobs).Build();
			IUserRepository userRepo = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();
			ResourceUtils resourceUtils = new ResourceUtilsBuilder().WithReadResourceReturns(ResourceKey.DigestEmailTemplate, emailBody).Build();
			ISmtpClientWrapper smtpClient = new SmtpClientWrapperBuilder().Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
				.WithBuildDefaultMailMessageReturns(mailMessage)
				.WithBuildSmtpHostFromSettingsReturns(smtpHost)
				.Build();

			// execute
			DigestJob digestJob = CreateDigestJob(dbContextFactory, resourceUtils, jobRepo, userRepo, smtpClient, globalUserSettingProvider);
			digestJob.Execute(Substitute.For<IJobExecutionContext>()).GetAwaiter().GetResult();

			// assert
			Assert.That(mailMessage.To.Single().Address, Is.EqualTo(adminUsers.Single().Email));
			Assert.That(mailMessage.Subject, Is.EqualTo(DigestJob.DigestJobEmailSubject));
			Assert.That(mailMessage.Body, Is.EqualTo(emailBody));
		}

		[Test]
		public void Execute_FailingJobs_AllAdministratorsEmailed()
		{
			// setup
			List<JobEntity> failingJobs = new List<JobEntity>
			{
				new JobEntityBuilder().WithRandomProperties().Build()
			};

			int adminCount = Faker.RandomNumber.Next(2, 10);
			List<UserEntity> adminUsers = new List<UserEntity>();
			for (int i=0; i< adminCount; i++) {
				adminUsers.Add(new UserEntityBuilder().WithRandomProperties().Build());
			};

			MailMessage mailMessage = new MailMessage();
			SmtpHost smtpHost = new SmtpHostBuilder().WithRandomProperties().Build();
			string emailBody = Faker.Lorem.Paragraph();

			IDbContext dbContext = new DbContextBuilder().Build();
			IDbContextFactory dbContextFactory = new DbContextFactoryBuilder().WithDbContext(dbContext).Build();
			JobRepository jobRepo = new JobRepositoryBuilder().WithGetAllFailingActiveAsyncReturns(dbContext, failingJobs).Build();
			IUserRepository userRepo = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();
			ResourceUtils resourceUtils = new ResourceUtilsBuilder().WithReadResourceReturns(ResourceKey.DigestEmailTemplate, emailBody).Build();
			ISmtpClientWrapper smtpClient = new SmtpClientWrapperBuilder().Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
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
			List<JobEntity> failingJobs = new List<JobEntity>
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
			JobRepository jobRepo = new JobRepositoryBuilder()
				.WithGetAllFailingActiveAsyncReturns(dbContext, failingJobs)
				.WithGetAllFailedSinceAsyncReturns(dbContext, Arg.Any<DateTime>(), recentlyFailedJobs)
				.Build();
			IUserRepository userRepo = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();
			ResourceUtils resourceUtils = new ResourceUtilsBuilder().WithReadResourceReturns(ResourceKey.DigestEmailTemplate, emailBody).Build();
			ISmtpClientWrapper smtpClient = new SmtpClientWrapperBuilder().Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
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
			List<JobEntity> failingJobs = new List<JobEntity>
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
			JobRepository jobRepo = new JobRepositoryBuilder().WithGetAllFailingActiveAsyncReturns(dbContext, failingJobs).Build();
			IUserRepository userRepo = new UserRepositoryBuilder().WithGetUsersInRoleAsyncReturns(UserRoles.Admin, adminUsers).Build();
			ResourceUtils resourceUtils = new ResourceUtilsBuilder().WithReadResourceReturns(ResourceKey.DigestEmailTemplate, emailBody).Build();
			ISmtpClientWrapper smtpClient = new SmtpClientWrapperBuilder().Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
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
