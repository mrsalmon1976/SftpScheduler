using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        [Test]
        public async Task Login_Integration_ReturnsOk()
        {
            using (var client = TestFactory.CreateHttpClient())
            {

                var response = await client.GetAsync("/auth/login");
                string body = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
