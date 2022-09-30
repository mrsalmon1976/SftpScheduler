using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Controllers
{
    public class TestController : CustomBaseController
    {

        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            if (this.HttpContext.Request.Headers["SftpScheduler-Test"] != "1")
            {
                return this.BadRequest("This page is for unit testing only and cannot be rendered in a browser.");
            }
            return this.View();
        }

    }
}
