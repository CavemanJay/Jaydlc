using System;

namespace Jaydlc.Core.Models
{
    public record Thumbnail
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public string resolution { get; set; }
        public string id { get; set; }

        public bool IsJpeg => url.Contains(".jpg");

        public string? JpegUrl =>
            url.Substring(0, url.IndexOf(".jpg") + 3);
    }
}