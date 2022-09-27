using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public ViewResult Login()
        {
            return this.View("Login/Index");
        }
    }
}
