namespace Jaydlc.Core.Models
{
    public class Format
    {
        public string format_id { get; set; } 
        public string manifest_url { get; set; } 
        public string ext { get; set; } 
        public int? width { get; set; } 
        public int? height { get; set; } 
        public double tbr { get; set; } 
        public int? asr { get; set; } 
        public int? fps { get; set; } 
        public object language { get; set; } 
        public string format_note { get; set; } 
        public int? filesize { get; set; } 
        public string container { get; set; } 
        public string vcodec { get; set; } 
        public string acodec { get; set; } 
        public string url { get; set; } 
        public string fragment_base_url { get; set; } 
        // public List<Fragment> fragments { get; set; } 
        public string protocol { get; set; } 
        public string format { get; set; } 
        // public HttpHeaders http_headers { get; set; } 
        public int? quality { get; set; } 
        public double? abr { get; set; } 
        // public DownloaderOptions downloader_options { get; set; } 
        public double? vbr { get; set; }     }
}