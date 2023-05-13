using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Common.Models
{
    public class VersionComparisonResult
    {
        public bool IsNewVersionAvailable { get; set; }

        public ApplicationVersionInfo? LatestReleaseVersionInfo { get; set; }
    }
}
