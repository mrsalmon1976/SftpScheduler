using Microsoft.Extensions.Configuration;

namespace SftpSchedulerService.Config
{
    public class AppSettings
    {
        private readonly IConfiguration _configuration;
        private readonly string _baseDirectory;

        public AppSettings(IConfiguration configurationManager, string baseDirectory)
        {
            this._configuration = configurationManager;
            this._baseDirectory = baseDirectory;
        }

        public virtual string DbPath
        {
            get
            {
                string dbPath = _configuration["AppSettings:DbPath"];
                if (dbPath.StartsWith(".\\") || dbPath.StartsWith("./"))
                {
                    dbPath = dbPath.Substring(2);
                    dbPath = Path.Combine(_baseDirectory, dbPath);
                }
                return dbPath.Replace("{AppDir}", _baseDirectory); ;
            }
        }

        public virtual string DbConnectionString
        {
            get
            {
                string connString = _configuration.GetConnectionString("Default");
                return connString.Replace("{AppDir}", _baseDirectory);
            }
        }

        public virtual string JwtSecret
        {
            get
            {
                return _configuration["JWT:Secret"];
            }
        }

        public virtual string JwtValidAudience
        {
            get
            {
                return _configuration["JWT:ValidAudience"];
            }
        }

        public virtual string JwtValidIssuer
        {
            get
            {
                return _configuration["JWT:ValidIssuer"];
            }
        }

    }
}
