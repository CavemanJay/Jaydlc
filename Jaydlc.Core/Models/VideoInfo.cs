using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jaydlc.Core.Models
{
    public record VideoInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public List<Thumbnail> Thumbnails { get; set; }

        // public List<Format> formats { get; set; }
        public string Description { get; set; }

        [JsonPropertyName("upload_date")]
        public string UploadDate { get; set; }

        // public string uploader { get; set; }
        // public string uploader_id { get; set; }
        // public string uploader_url { get; set; }
        // public string channel_id { get; set; }
        // public string channel_url { get; set; }
        [JsonPropertyName("duration")]
        public double DurationInSeconds { get; set; }

        public int view_count { get; set; }
        // public double average_rating { get; set; }
        // public int age_limit { get; set; }

        [JsonPropertyName("webpage_url")]
        public string VideoLink { get; set; }
        // public List<string> categories { get; set; }
        // public List<object> tags { get; set; }
        // public object is_live { get; set; }
        // public int like_count { get; set; }
        // public int dislike_count { get; set; }
        // public object channel { get; set; }
        // public string extractor { get; set; }
        // public string webpage_url_basename { get; set; }
        // public string extractor_key { get; set; }
        // public int n_entries { get; set; }
        // public string playlist { get; set; }

        [JsonPropertyName("playlist_id")]
        public string PlaylistId { get; set; }

        [JsonPropertyName("playlist_title")]
        public string PlaylistTitle { get; set; }

        // public string playlist_uploader { get; set; }
        // public string playlist_uploader_id { get; set; }
        public int Playlist_index { get; set; }
        public string Thumbnail { get; set; }

        public string Display_id { get; set; }

        // public int asr { get; set; }
        // public object filesize { get; set; }
        // public string format_id { get; set; }
        // public string format_note { get; set; }
        public int Fps { get; set; }
        public int Height { get; set; }

        public int Quality { get; set; }

        // public double tbr { get; set; }
        public string url { get; set; }

        // public int width { get; set; }
        // public string ext { get; set; }
        // public string vcodec { get; set; }
        // public string acodec { get; set; }
        // public string format { get; set; }
        public string protocol { get; set; }
        public string FullTitle { get; set; }
        public string _filename { get; set; }
    }
}