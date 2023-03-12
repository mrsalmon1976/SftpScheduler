using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Controllers
{
    public class AuthController : CustomBaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CreateUserCommand _createUserCommand;

        public AuthController(UserManager<IdentityUser> userManager, CreateUserCommand createUserCommand) 
        {
            _userManager = userManager;
            _createUserCommand = createUserCommand;
        }

        [HttpGet]
        public ViewResult Login()
        {
            return this.View("Login");
        }
    }
}
