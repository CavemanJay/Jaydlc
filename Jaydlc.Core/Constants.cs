using System;
using System.Runtime.InteropServices;

namespace Jaydlc.Core
{
    public static class Constants
    {
        public static string TempFolder =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.ExpandEnvironmentVariables("%TEMP%")
                : "/tmp";
    }
}