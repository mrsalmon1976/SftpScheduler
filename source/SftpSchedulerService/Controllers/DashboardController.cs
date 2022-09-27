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
    public class DashboardController : Controller
    {

        [HttpGet]
        public ViewResult Index()
        {
            return this.View();
        }

    }
}
