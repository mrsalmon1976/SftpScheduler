using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SftpScheduler.BLL.Commands.Notification;
using SftpScheduler.BLL.Commands.Setting;
using SftpScheduler.BLL.Config;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Settings;

namespace SftpSchedulerService.ViewOrchestrators.Api.Settings
{
    public interface ISettingsUpdateOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(GlobalSettingsViewModel globalSettingsViewModel);
    }

    public class SettingsUpdateOrchestrator : ISettingsUpdateOrchestrator
    {
		private readonly IDbContextFactory _dbContextFactory;
		private readonly IGlobalUserSettingProvider _globalUserSettingProvider;
		private readonly IStartupSettingProvider _startupSettingProvider;
		private readonly IUpsertDigestCommand _upsertDigestCommand;
		private readonly IUpsertGlobalUserSettingCommand _upsertGlobalUserSettingCommand;

		public SettingsUpdateOrchestrator(IDbContextFactory dbContextFactory, IGlobalUserSettingProvider globalUserSettingProvider, IStartupSettingProvider startupSettingProvider, IUpsertDigestCommand upsertDigestCommand, IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand)
        {
			_dbContextFactory = dbContextFactory;
			_globalUserSettingProvider = globalUserSettingProvider;
			_startupSettingProvider = startupSettingProvider;
			_upsertDigestCommand = upsertDigestCommand;
			_upsertGlobalUserSettingCommand = upsertGlobalUserSettingCommand;
		}

        public async Task<IActionResult> Execute(GlobalSettingsViewModel globalSettingsViewModel)
        {
			// startup settings first - only save if they have changed
			var currentStartupSettings = _startupSettingProvider.Load();
			StartupSettings newStartupSettings = new StartupSettings();
			SettingsModelMapper.MapValues(globalSettingsViewModel, newStartupSettings);

            if (!currentStartupSettings.Equals(newStartupSettings))
            {
				await _startupSettingProvider.Save(newStartupSettings);
			}

			using IDbContext dbContext = _dbContextFactory.GetDbContext();

			GlobalSettingsViewModel currentStoredSettings = new GlobalSettingsViewModel();
			SettingsModelMapper.MapValues(_globalUserSettingProvider, currentStoredSettings);

			// check digest values - if either of these change we need to update the digest job
			bool digestTimeChanged = await _globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.DigestTime, currentStoredSettings.DigestTime, globalSettingsViewModel.DigestTime);
			bool digestDaysChanged = await _globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.DigestDays, currentStoredSettings.DigestDays, globalSettingsViewModel.DigestDays);
			
			if (digestDaysChanged || digestTimeChanged) 
			{
				IEnumerable<DayOfWeek> digestDays = globalSettingsViewModel.DigestDays.Select(x => Enum.Parse<DayOfWeek>(x));
				await _upsertDigestCommand.Execute(digestDays, globalSettingsViewModel.DigestTime);
			}

			// run in all the simple settings - these will only update if their value has changed
			await _globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpHost, currentStoredSettings.SmtpHost, globalSettingsViewModel.SmtpHost);
			await _globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpPort, currentStoredSettings.SmtpPort, globalSettingsViewModel.SmtpPort);
			await _globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpUserName, currentStoredSettings.SmtpUserName, globalSettingsViewModel.SmtpUserName);
			await _globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpPassword, currentStoredSettings.SmtpPassword, globalSettingsViewModel.SmtpPassword);
			await _globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpEnableSsl, currentStoredSettings.SmtpEnableSsl, globalSettingsViewModel.SmtpEnableSsl);
			await _globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpFromName, currentStoredSettings.SmtpFromName, globalSettingsViewModel.SmtpFromName);
			await _globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpFromEmail, currentStoredSettings.SmtpFromEmail, globalSettingsViewModel.SmtpFromEmail);

			return new OkResult();
        }

		

	}
}
