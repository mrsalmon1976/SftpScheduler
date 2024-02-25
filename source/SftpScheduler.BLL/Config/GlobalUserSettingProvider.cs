using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SftpScheduler.BLL.Commands.Setting;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Security;
using SftpScheduler.BLL.Utility;
using System.Net.Mail;

namespace SftpScheduler.BLL.Config
{
	public interface IGlobalUserSettingProvider
	{
		DayOfWeek[] DigestDays { get; set; }

		int DigestTime { get; set; }

		string SmtpHost { get; set; }

		int SmtpPort { get; set; }

		string SmtpUserName { get; set; }

		string SmtpPassword { get; set; }

		bool SmtpEnableSsl { get; set; }

		string SmtpFromName { get; set; }

		string SmtpFromEmail { get; set; }

		MailMessage BuildDefaultMailMessage();

		SmtpHost BuildSmtpHostFromSettings();

		string DecryptSmtpPassword();

		Task<bool> UpdateChangedValue(IDbContext dbContext, string settingKey, string oldValue, string newValue);

		Task<bool> UpdateChangedValue(IDbContext dbContext, string settingKey, string[] oldValue, string[] newValue);

		Task<bool> UpdateChangedValue(IDbContext dbContext, string settingKey, int oldValue, int newValue);

		Task<bool> UpdateChangedValue(IDbContext dbContext, string settingKey, bool oldValue, bool newValue);
	}

	public class GlobalUserSettingProvider : IGlobalUserSettingProvider
	{
		private readonly ILogger<GlobalUserSettingProvider> _logger;
		private readonly IDbContextFactory _dbContextFactory;
		private readonly IEncryptionProvider _encryptionProvider;
		private readonly IUpsertGlobalUserSettingCommand _upsertGlobalUserSettingCommand;
		private readonly GlobalUserSettingRepository _globalUserSettingRepository;

		public static readonly DayOfWeek[] DefaultDigestDays = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

		public const int DefaultDigestTime = 17;

		public const int DefaultSmtpPort = 25;

		public GlobalUserSettingProvider(ILogger<GlobalUserSettingProvider> logger, IDbContextFactory dbContextFactory, IEncryptionProvider encryptionProvider, IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand, GlobalUserSettingRepository globalUserSettingRepository)
		{
			_logger = logger;
			_dbContextFactory = dbContextFactory;
			_encryptionProvider = encryptionProvider;
			_upsertGlobalUserSettingCommand = upsertGlobalUserSettingCommand;
			_globalUserSettingRepository = globalUserSettingRepository;

			this.LoadAllGlobalUserSettings();
		}

		public DayOfWeek[] DigestDays { get; set; } = DefaultDigestDays;


		public int DigestTime { get; set; }

		public string SmtpHost { get; set; } = String.Empty;

		public int SmtpPort { get; set; } = DefaultSmtpPort;

		public string SmtpUserName { get; set; } = String.Empty;

		public string SmtpPassword { get; set; } = String.Empty;

		public bool SmtpEnableSsl { get; set; } = true;

		public string SmtpFromName { get; set; } = String.Empty;

		public string SmtpFromEmail { get; set; } = String.Empty;

		public MailMessage BuildDefaultMailMessage()
		{
			MailMessage message = new MailMessage();
			message.From = new MailAddress(this.SmtpFromEmail, this.SmtpFromName);
			message.IsBodyHtml = true;
			return message;
		}

		public SmtpHost BuildSmtpHostFromSettings()
		{
			SmtpHost host = new SmtpHost
			{
				EnableSsl = this.SmtpEnableSsl,
				Host = this.SmtpHost,
				Password = this.DecryptSmtpPassword(),
				Port = this.SmtpPort,
				UserName = this.SmtpUserName
			};
			return host;
		}

		public string DecryptSmtpPassword()
		{
			return _encryptionProvider.Decrypt(this.SmtpPassword);
		}


		private T GetSettingValue<T>(IEnumerable<GlobalUserSettingEntity> allSettings, string settingId, T defaultValue)
		{
			try
			{
				GlobalUserSettingEntity? setting = allSettings.SingleOrDefault(x => x.Id == settingId);
				if (setting == null)
				{
					return defaultValue;
				}

				// for array types, we serialize it
				if (typeof(T).IsArray)
				{
					var result = JsonConvert.DeserializeObject<T>(setting.SettingValue);
					if (result == null)
					{
						return defaultValue;
					}
					return result;
				}

				return (T)Convert.ChangeType(setting.SettingValue, typeof(T));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message, ex);
				return defaultValue;
			}
		}

		private void LoadAllGlobalUserSettings()
		{
			using (IDbContext dbContext = _dbContextFactory.GetDbContext())
			{
				IEnumerable<GlobalUserSettingEntity> allSettings = _globalUserSettingRepository.GetAllAsync(dbContext).GetAwaiter().GetResult();

				this.DigestDays = GetSettingValue(allSettings, GlobalUserSettingKey.DigestDays, DefaultDigestDays);
				this.DigestTime = GetSettingValue(allSettings, GlobalUserSettingKey.DigestTime, DefaultDigestTime);
				this.SmtpHost = GetSettingValue(allSettings, GlobalUserSettingKey.SmtpHost, String.Empty);
				this.SmtpPort = GetSettingValue(allSettings, GlobalUserSettingKey.SmtpPort, DefaultSmtpPort);
				this.SmtpUserName = GetSettingValue(allSettings, GlobalUserSettingKey.SmtpUserName, String.Empty);
				this.SmtpPassword = GetSettingValue(allSettings, GlobalUserSettingKey.SmtpPassword, String.Empty);
				this.SmtpEnableSsl = GetSettingValue(allSettings, GlobalUserSettingKey.SmtpEnableSsl, true);
				this.SmtpFromName = GetSettingValue(allSettings, GlobalUserSettingKey.SmtpFromName, String.Empty);
				this.SmtpFromEmail = GetSettingValue(allSettings, GlobalUserSettingKey.SmtpFromEmail, String.Empty);
			}
		}

		public async Task<bool> UpdateChangedValue(IDbContext dbContext, string settingKey, string oldValue, string newValue)
		{
			if (oldValue.Equals(newValue))
			{
				return false;
			}

			string storedValue = newValue;

			// passwords get encrypted!
			if (settingKey == GlobalUserSettingKey.SmtpPassword)
			{
				storedValue = _encryptionProvider.Encrypt(storedValue);
			}

			await _upsertGlobalUserSettingCommand.Execute(dbContext, new GlobalUserSettingEntity(settingKey, storedValue));
			return true;
		}

		public async Task<bool> UpdateChangedValue(IDbContext dbContext, string settingKey, string[] oldValue, string[] newValue)
		{
			bool valueChanged = !ArrayUtils.ElementsEqualUnordered(oldValue, newValue);

			if (valueChanged)
			{
				await _upsertGlobalUserSettingCommand.Execute(dbContext, new GlobalUserSettingEntity(settingKey, JsonConvert.SerializeObject(newValue)));
			}

			return valueChanged;
		}


		public async Task<bool> UpdateChangedValue(IDbContext dbContext, string settingKey, bool oldValue, bool newValue)
		{
			if (oldValue.Equals(newValue))
			{
				return false;
			}

			await _upsertGlobalUserSettingCommand.Execute(dbContext, new GlobalUserSettingEntity(settingKey, newValue.ToString()));
			return true;
		}

		public async Task<bool> UpdateChangedValue(IDbContext dbContext, string settingKey, int oldValue, int newValue)
		{
			if (oldValue.Equals(newValue))
			{
				return false;
			}

			await _upsertGlobalUserSettingCommand.Execute(dbContext, new GlobalUserSettingEntity(settingKey, newValue.ToString()));
			return true;
		}


	}


}
