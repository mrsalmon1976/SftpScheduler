using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class UpdateController : CustomBaseController
    {

        [HttpGet]
        public ViewResult Install()
        {
            return this.View();
        }

    }
}
