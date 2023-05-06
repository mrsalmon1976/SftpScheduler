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
    public class UsersController : CustomBaseController
    {

        [HttpGet]
        public ViewResult Index()
        {
            return this.View();
        }

        [HttpGet("users/create")]
        public ViewResult Create()
        {
            return this.View("Form", new { UserId = "" });
        }

    }
}
