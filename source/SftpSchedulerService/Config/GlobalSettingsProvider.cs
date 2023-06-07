using Newtonsoft.Json;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.Common.IO;

namespace SftpSchedulerService.Config
{
    public interface IGlobalSettingsProvider
    {
        string FilePath { get; }

        GlobalSettings Load();


    }

    public class GlobalSettingsProvider : IGlobalSettingsProvider
    {
        private readonly IDirectoryUtility _dirUtility;
        private readonly IFileUtility _fileUtility;

        public GlobalSettingsProvider(IDirectoryUtility dirUtility, IFileUtility fileUtility)
        {
            _dirUtility = dirUtility;
            _fileUtility = fileUtility;
        }

        public GlobalSettingsProvider(AppSettings appSettings, IDirectoryUtility dirUtility, IFileUtility fileUtility) 
        {
            _dirUtility = dirUtility;
            _fileUtility = fileUtility;

            FilePath = Path.Combine(appSettings.DataDirectory, "global.json");
        }

        //private T GetDefaultValue<T>(GlobalSettingKey settingKey)
        //{
        //    switch (settingKey)
        //    {
        //        case GlobalSettingKey.MaxConcurrentJobs:
        //            return (T)Convert.ChangeType(DefaultMaxConcurrentJobs, typeof(T));
        //        default:
        //            throw new NotSupportedException($"Unsupported setting key {settingKey}");
        //    }
        //}

        //private T GetValue<T>(IEnumerable<GlobalSettingEntity> settings, GlobalSettingKey settingKey, T defaultValue)
        //{

        //    GlobalSettingEntity? setting = settings.SingleOrDefault(s => s.Id == settingKey.ToString());
        //    if (setting != null)
        //    {
        //        return (T)Convert.ChangeType(setting.SettingValue, typeof(T));
        //    }
        //    return GetDefaultValue<T>(settingKey);
        //}

        public string FilePath { get; private set; }


        public GlobalSettings Load()
        {
            GlobalSettings? globalSettings = null;
            if (_fileUtility.Exists(this.FilePath))
            {
                string fileData = _fileUtility.ReadAllText(FilePath);
                globalSettings = JsonConvert.DeserializeObject<GlobalSettings>(fileData);
            }
            return globalSettings ?? new GlobalSettings();
        }

        public void Save(GlobalSettings globalSettings)
        {
            string dataDirectory = Path.GetDirectoryName(FilePath)!;
            _dirUtility.Create(dataDirectory);
            string fileData = JsonConvert.SerializeObject(globalSettings, Formatting.Indented);
            _fileUtility.WriteAllText(FilePath, fileData);
        }


    }
}
