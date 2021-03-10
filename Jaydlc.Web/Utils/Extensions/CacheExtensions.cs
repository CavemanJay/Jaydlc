using System;
using Microsoft.Extensions.Caching.Memory;

namespace Jaydlc.Web.Utils.Extensions
{
    public static class CacheExtensions
    {
        public static void SetRecord<T>(this IMemoryCache cache,
            string recordId, T data, TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime ??
                                                  Constants.DefaultCacheTimeout,
                SlidingExpiration = unusedExpireTime,
            };

            cache.Set(recordId, data, options);
        }

        public static T? GetRecord<T>(this IMemoryCache cache,
            string recordId)
        {
            var data = cache.Get<T>(recordId);

            // Return the default value of type T if the record doesn't exist
            // Otherwise return the deserialized object
            return data is null ? default : data;
        }
    }
}