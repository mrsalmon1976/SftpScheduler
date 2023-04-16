using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SftpSchedulerService.Utilities;
using System.Net.Http;
using System.Text;

namespace SftpSchedulerService.TagHelpers
{
    [HtmlTargetElement("environment-script")]

    public class EnvironmentScriptTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment _environment;
        private HttpContext? _httpContext;

        public EnvironmentScriptTagHelper(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor) 
        {
            _httpContext = httpContextAccessor.HttpContext;
            _environment = environment;
            
            this.Src = "";
            this.ProdSuffix = ".min.js";
            this.DevSuffix = ".js";
            this.AppendAppVersion = true;
        }

        public string Src { get; set; }

        public string ProdSuffix { get; set; }

        public string DevSuffix { get; set; }

        public bool AppendAppVersion { get; set; }  

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string src = this.Src;
            if (_httpContext != null && !String.IsNullOrEmpty(src) && src.StartsWith("~")) 
            {
                src = _httpContext.Request.PathBase + src.Substring(1);
            }

            if (_environment.IsDevelopment() && src.EndsWith(this.ProdSuffix))
            {
                src = src.Replace(this.ProdSuffix, this.DevSuffix);
            }
            else if (_environment.IsProduction() && !src.EndsWith(this.ProdSuffix))
            {
                src = src.Replace(this.DevSuffix, this.ProdSuffix);
            }

            // append application version to the script so caching doesn't happen
            if (this.AppendAppVersion)
            {
                src = $"{src}?v={AppUtils.Version}";
            }

            output.TagName = "script";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("src", src);
        }
    }



}
