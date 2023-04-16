using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SftpSchedulerService.Utilities;
using System.Net.Http;
using System.Text;

namespace SftpSchedulerService.TagHelpers
{
    [HtmlTargetElement("environment-style")]

    public class EnvironmentStyleTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment _environment;
        private HttpContext? _httpContext;

        public EnvironmentStyleTagHelper(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor) 
        {
            _httpContext = httpContextAccessor.HttpContext;
            _environment = environment;
            
            this.Rel = "stylesheet";
            this.Href = "";
            this.ProdSuffix = ".min.css";
            this.DevSuffix = ".css";
            this.AppendAppVersion = true;
        }

        public string Rel { get; set; }

        public string Href { get; set; }

        public string ProdSuffix { get; set; }

        public string DevSuffix { get; set; }

        public bool AppendAppVersion { get; set; }  

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string href = this.Href;
            if (_httpContext != null && !String.IsNullOrEmpty(href) && href.StartsWith("~")) 
            {
                href = _httpContext.Request.PathBase + href.Substring(1);
            }

            if (_environment.IsDevelopment() && href.EndsWith(this.ProdSuffix))
            {
                href = href.Replace(this.ProdSuffix, this.DevSuffix);
            }
            else if (_environment.IsProduction() && !href.EndsWith(this.ProdSuffix))
            {
                href = href.Replace(this.DevSuffix, this.ProdSuffix);
            }

            // append application version to the script so caching doesn't happen
            if (this.AppendAppVersion)
            {
                href = $"{href}?v={AppUtils.Version}";
            }

            output.TagName = "link";
            output.TagMode = TagMode.SelfClosing;
            output.Attributes.Add("rel", this.Rel);
            output.Attributes.Add("href", href);
        }
    }



}
