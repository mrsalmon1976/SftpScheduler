namespace SftpSchedulerService.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        [Test]
        public async Task Login_Integration_ReturnsOk()
        {

            using (var client = HttpClientTestFactory.CreateHttpClient())
            {

                var response = await client.GetAsync("/auth/login");
                string body = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
