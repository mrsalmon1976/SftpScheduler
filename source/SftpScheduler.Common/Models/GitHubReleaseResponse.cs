using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.Common.Models
{
    public class GitHubReleaseResponse
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [JsonProperty("assets")]
        public GitHubAsset[] Assets { get; set; } = Array.Empty<GitHubAsset>();

        public GitHubAsset? GetApplicationAsset()
        {
            if (this.Assets == null || this.Assets.Length == 0)
            {
                return null;
            }
            return this.Assets.FirstOrDefault(x => x.Name!.StartsWith("SftpScheduler_"));
        }

    }
}
