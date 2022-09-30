using HtmlAgilityPack;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using SftpSchedulerService.BLL.Identity;
using SftpSchedulerService.Config;
using SftpSchedulerService.Tests;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Test.SftpSchedulerService.Views.Shared
{
    [TestFixture]
    public class _LayoutTests
    {

        [Test]
        public async Task MenuItem_UserIsNotAdmin_UsersMenuItemNotRendered()
        {
            string[] roles = { UserRoles.User };
            var client = TestFactory.CreateAuthenticatedHttpClient(roles);

            var response = await client.GetAsync("/test");
            string body = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(body);

            var node = htmlDoc.GetElementbyId("menu-users");
            Assert.IsNull(node);
        }

        [Test]
        public async Task MenuItem_UserIsAdmin_UsersMenuItemIsRendered()
        {
            string[] roles = { UserRoles.Admin };
            var client = TestFactory.CreateAuthenticatedHttpClient(roles);

            var response = await client.GetAsync("/dashboard");
            string body = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(body);

            var node = htmlDoc.GetElementbyId("menu-users");
            Assert.IsNotNull(node);
        }


    }
}
