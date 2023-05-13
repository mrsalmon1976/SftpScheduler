using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace SftpSchedulerService.AutoUpdater.Config
{
    public class AppSettings
    {
        private readonly IConfiguration _configuration;

        public AppSettings(IConfiguration configurationManager)
        {
            this._configuration = configurationManager;
        }

        public virtual string LatestVersionUrl
        {
            get
            {
                return _configuration["AppSettings:LatestVersionUrl"];
            }
        }
    }
}
