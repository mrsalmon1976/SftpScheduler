namespace SftpSchedulerService.Config
{
    public class AppSettings
    {
        private readonly ConfigurationManager _configurationManager;
        private readonly string _baseDirectory;

        public AppSettings(ConfigurationManager configurationManager, string baseDirectory)
        {
            this._configurationManager = configurationManager;
            this._baseDirectory = baseDirectory;
        }

        public virtual string DbPath
        {
            get
            {
                string dbPath = _configurationManager["AppSettings:DbPath"];
                if (dbPath.StartsWith(".\\") || dbPath.StartsWith("./"))
                {
                    dbPath = dbPath.Substring(2);
                    dbPath = Path.Combine(_baseDirectory, dbPath);
                }
                return dbPath;
            }
        }

    }
}
