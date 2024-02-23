using Microsoft.Extensions.Logging;
using Quartz;
using SftpScheduler.BLL.Config;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Net;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Utility;
using System.Net.Mail;

namespace SftpScheduler.BLL.Jobs
{
    public class DigestJob : IJob
    {
        public const string GroupName = "DigestJob";

		public const string DigestJobEmailSubject = "SftpScheduler Digest Notification";

		public const string FailingJobsTag = "{{FailingJobs}}";

		public const string RecentlyFailedJobsTag = "{{RecentlyFailedJobs}}";


        private readonly ILogger<DigestJob> _logger;
        private readonly IDbContextFactory _dbContextFactory;
		private readonly ResourceUtils _resourceUtils;
		private readonly JobRepository _jobRepository;
		private readonly IUserRepository _userRepository;
		private readonly ISmtpClientWrapper _smtpClient;
		private readonly IGlobalUserSettingProvider _globalUserSettingProvider;

		public DigestJob(ILogger<DigestJob> logger
            , IDbContextFactory dbContextFactory
			, ResourceUtils resourceUtils
			, JobRepository jobRepository
			, IUserRepository userRepository
			, ISmtpClientWrapper smtpClient
			, IGlobalUserSettingProvider globalUserSettingProvider
            ) 
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
			_resourceUtils = resourceUtils;
			_jobRepository = jobRepository;
			_userRepository = userRepository;
			_smtpClient = smtpClient;
			_globalUserSettingProvider = globalUserSettingProvider;
		}

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Digest job execution started");

			// if SMTP not configured, exit
			if (String.IsNullOrWhiteSpace(_globalUserSettingProvider.SmtpHost))
			{
				_logger.LogInformation("Global SMTP not set up - exiting digest job");
				return;
			}

			var admins = await _userRepository.GetUsersInRoleAsync(UserRoles.Admin);
			var adminEmailAddresses = String.Join(',', admins.Where(x => x.LockoutEnd == null || x.LockoutEnd < DateTime.UtcNow).Select(x => x.Email));

			// if there are no admin mail addresses set, then exit
			if (String.IsNullOrWhiteSpace(adminEmailAddresses))
			{
				_logger.LogInformation("No administrators with an email address - exiting digest job");
				return;
			}

			// we're configured to send, load up the HTML and release the hounds
			string emailBodyHtml = _resourceUtils.ReadResource(ResourceKey.DigestEmailTemplate);
			string recentlyFailedJobsHtml = "<li>No jobs have failed recently</li>";

			using (IDbContext dbContext = _dbContextFactory.GetDbContext())
			{
				var failingJobs = await _jobRepository.GetAllFailingActiveAsync(dbContext);
				if (!failingJobs.Any())
				{
					_logger.LogInformation("No failing jobs, digest email not sent");
					return;
				}

				var recentlyFailedJobs = await _jobRepository.GetAllFailedSinceAsync(dbContext, DateTime.Now.AddDays(-1));

				string failingJobsHtml = String.Join(' ', failingJobs.Select(x => $"<li>{x.Name}</li>"));
				if (recentlyFailedJobs.Any() )
				{
					recentlyFailedJobsHtml = String.Join(' ', recentlyFailedJobs.Select(x => $"<li>{x.Name}</li>"));
				}

				emailBodyHtml = emailBodyHtml.Replace(FailingJobsTag, failingJobsHtml);
				emailBodyHtml = emailBodyHtml.Replace(RecentlyFailedJobsTag, recentlyFailedJobsHtml);
			}

			MailMessage mailMessage = _globalUserSettingProvider.BuildDefaultMailMessage();
			mailMessage.To.Add(adminEmailAddresses);
			mailMessage.Subject = $"{DigestJobEmailSubject} : {DateTime.Now.ToString("yyyy-MM-dd")}";
			mailMessage.Body = emailBodyHtml;

			_smtpClient.Send(_globalUserSettingProvider.BuildSmtpHostFromSettings(), mailMessage);

			_logger.LogInformation("Digest email sent to {adminEmailAddresses}", adminEmailAddresses);

		}

		public static string JobKeyName()
        {
            return "DigestJob";
        }

        public static string TriggerKeyName()
        {
            return "DigestTrigger";
        }

    }
}
