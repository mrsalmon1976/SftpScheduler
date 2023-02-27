using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Controllers
{
    public class CustomBaseController : Controller
    {

        public string? Version
        {
            get
            {
                if (Debugger.IsAttached)
                {
                    return DateTime.Now.ToString("yyyyMMddHHmmssttt");
                }

                return Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString(3);
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            ViewBag.Version = this.Version;
        }

    }
}
