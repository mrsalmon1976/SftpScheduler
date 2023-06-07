using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace SftpSchedulerService.Config
{
    public class AppSettings
    {
        private readonly IConfiguration _configuration;

        private string? _dbPath;
        private string? _dbPathQuartz;
        
        public AppSettings()
        {
            // this constructor for testing only
            this._configuration = null!;
            this.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        public AppSettings(IConfiguration configurationManager, string baseDirectory)
        {
            this._configuration = configurationManager;
            this.BaseDirectory = baseDirectory;
        }

        public virtual string BaseDirectory { get; private set; }

        public virtual string DataDirectory
        {
            get
            {
                return Path.Combine(BaseDirectory, "Data");
            }
        }

        public virtual string DbPath
        {
            get
            {
                if (String.IsNullOrEmpty(_dbPath))
                {
                    DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                    builder.ConnectionString = this.DbConnectionString;
                    _dbPath = ((string)builder["Data Source"]).Trim();
                }
                return _dbPath;
            }
        }

        public virtual string DbPathQuartz
        {
            get
            {
                if (String.IsNullOrEmpty(_dbPathQuartz))
                {
                    DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                    builder.ConnectionString = this.DbConnectionStringQuartz;
                    _dbPathQuartz = ((string)builder["Data Source"]).Trim();
                }
                return _dbPathQuartz;
            }
        }

        public virtual string DbConnectionString
        {
            get
            {
                string connString = _configuration.GetConnectionString("Default");
                return connString.Replace("{AppDir}", BaseDirectory);
            }
        }

        public virtual string DbConnectionStringQuartz
        {
            get
            {
                string connString = _configuration.GetConnectionString("Quartz");
                return connString.Replace("{AppDir}", BaseDirectory);
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

        public virtual string LatestVersionUrl
        {
            get
            {
                return _configuration["AppSettings:LatestVersionUrl"];
            }
        }

        public virtual int Port
        {
            get
            {
                return Convert.ToInt32(_configuration["AppSettings:Port"]);
            }
        }

        public virtual string SecretKey
        {
            get
            {
                return _configuration["AppSettings:SecretKey"];
            }
        }

        public virtual int UpdateCheckIntervalInMinutes
        {
            get
            {
                return Convert.ToInt32(_configuration["AppSettings:UpdateCheckIntervalInMinutes"]);
            }
        }

    }
}
