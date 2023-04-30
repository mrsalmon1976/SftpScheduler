using Microsoft.Extensions.Caching.Memory;

namespace SftpSchedulerService.Caching
{
    public interface ICacheProvider
    {
        T Get<T>(string key) where T : class;

        void Set(string key, object value, TimeSpan absoluteExpirationRelativeToNow);
    }

    public class CacheProvider : ICacheProvider
    {
        private readonly IMemoryCache _memoryCache;

        public CacheProvider(IMemoryCache memoryCache) 
        {
            _memoryCache = memoryCache;
        }

        public T Get<T>(string key) where T : class 
        { 
            return _memoryCache.Get<T>(key);
        }

        public void Set(string key, object value, TimeSpan absoluteExpirationRelativeToNow)
        {
            _memoryCache.Set(key, value, absoluteExpirationRelativeToNow);
        }
    }
}
