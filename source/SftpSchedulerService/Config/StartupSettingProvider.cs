using Newtonsoft.Json;
using SftpScheduler.Common.IO;

namespace SftpSchedulerService.Config
{
    public interface IStartupSettingProvider
	{
        string FilePath { get; }

		StartupSettings Load();

        Task Save(StartupSettings settings);

    }

    public class StartupSettingProvider : IStartupSettingProvider
	{
        private readonly IDirectoryUtility _dirUtility;
        private readonly IFileUtility _fileUtility;

        private StartupSettings? _settings;


        public StartupSettingProvider(AppSettings appSettings, IDirectoryUtility dirUtility, IFileUtility fileUtility) 
        {
            _dirUtility = dirUtility;
            _fileUtility = fileUtility;

            FilePath = Path.Combine(appSettings.DataDirectory, "startup.settings.json");
        }

        public string FilePath { get; private set; }


        public StartupSettings Load()
        {
            if (_settings == null)
            {
                if (_fileUtility.Exists(this.FilePath))
                {
                    string fileData = _fileUtility.ReadAllText(FilePath);
                    _settings = JsonConvert.DeserializeObject<StartupSettings>(fileData);
                }
                _settings ??= new StartupSettings();
            }
            return _settings;
        }

        public async Task Save(StartupSettings settings)
        {
            string dataDirectory = Path.GetDirectoryName(FilePath)!;
            _dirUtility.Create(dataDirectory);
            string fileData = JsonConvert.SerializeObject(settings, Formatting.Indented);
            await _fileUtility.WriteAllTextAsync(FilePath, fileData);
            _settings = settings;
        }


    }
}
