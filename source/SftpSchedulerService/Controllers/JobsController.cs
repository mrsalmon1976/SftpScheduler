using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Controllers
{
    [Authorize]
    public class JobsController : CustomBaseController
    {

        [HttpGet]
        public ViewResult Index()
        {
            return this.View();
        }

        [HttpGet("jobs/{id}")]
        public ViewResult Detail(string id)
        {
            if (id.Equals("create", StringComparison.OrdinalIgnoreCase))
            {
                id = "";
            }
            return this.View("Form", new { JobId = id });
        }

    }
}
