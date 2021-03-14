using System;
using System.IO;
using System.Security.Cryptography;

namespace Jaydlc.Commander.Shared
{
    public static class Utils
    {
        public static string GetFileHash(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var md5 = MD5.Create();
            var md5Sum = BitConverter.ToString(md5.ComputeHash(stream))
                                     .Replace("-", "")
                                     .ToLower();

            return md5Sum;
        }
    }
}