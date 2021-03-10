using System;

namespace Jaydlc.Web.Utils
{
    public static class Constants
    {
        public static TimeSpan DefaultCacheTimeout =>
            TimeSpan.FromMinutes(30);

        public static TimeSpan RepositoryCacheTimeout =>
            TimeSpan.FromMinutes(20);
    }
}