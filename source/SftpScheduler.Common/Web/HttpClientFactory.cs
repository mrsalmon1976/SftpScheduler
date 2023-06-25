﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.Common.Web
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
    }

    public class HttpClientFactory : IHttpClientFactory
    {
        // HttpClient should be static, so this factory should be a singleton
        // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        private static HttpClient? _client;

        public HttpClientFactory()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "SftpSchedulerServiceUpdater");
        }

        public HttpClient GetHttpClient()
        {
            return _client!;
        }
    }
}
