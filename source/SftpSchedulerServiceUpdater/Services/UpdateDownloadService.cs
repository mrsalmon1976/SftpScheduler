namespace SftpSchedulerServiceUpdater.Services
{

    public interface IUpdateDownloadService
    {
        Task DownloadFile(string uri, string downloadPath);
    }

    public class UpdateDownloadService : IUpdateDownloadService
    {
        private readonly SftpScheduler.Common.Web.IHttpClientFactory _httpClientFactory;

        public UpdateDownloadService(SftpScheduler.Common.Web.IHttpClientFactory httpClientFactory)
        {
            this._httpClientFactory = httpClientFactory;
        }

        public async Task DownloadFile(string uri, string downloadPath)
        {
            HttpClient client = _httpClientFactory.GetHttpClient();
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            using (var fs = new FileStream(downloadPath, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
            }

            await Task.Yield();
        }
    }
}
