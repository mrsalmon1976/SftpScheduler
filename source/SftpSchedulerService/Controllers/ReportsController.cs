using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SftpSchedulerService.Controllers
{
    [Authorize]
    public class ReportsController : CustomBaseController
    {

        [HttpGet("reports/notransfers")]
        public ViewResult NoTransfers()
        {
            return this.View();
        }

    }
}
