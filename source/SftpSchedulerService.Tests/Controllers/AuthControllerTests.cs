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
            //using (var app = new WebApplicationFactory<Program>())
            //{
            //    using (var client = app.CreateClient())
            //    {
            //        var response = client.GetAsync("/auth/login").GetAwaiter().GetResult();
            //        var data = response.Content.ReadAsStringAsync().Result;
            //        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            //    }
            //}

            using (var client = TestFactory.CreateHttpClient())
            {

                var response = await client.GetAsync("/auth/login");
                string body = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
