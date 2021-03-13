using System;
using System.Runtime.InteropServices;

namespace Jaydlc.Web.Utils
{
    public static class Constants
    {
        public static TimeSpan DefaultCacheTimeout =>
            TimeSpan.FromMinutes(30);

        public static TimeSpan RepositoryCacheTimeout =>
            TimeSpan.FromMinutes(20);

        public static TimeSpan VideoSyncInterval =>
            TimeSpan.FromHours(8);

        public static string TempFolder =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.ExpandEnvironmentVariables("%TEMP%")
                : "/tmp";
    }
}