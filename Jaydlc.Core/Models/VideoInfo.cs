using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jaydlc.Core.Models
{
    public record VideoInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("title")]
        public string Title { get; init; }

        [JsonPropertyName("thumbnails")]
        public List<Thumbnail> Thumbnails { get; init; }

        [JsonPropertyName("formats")]
        public List<Format> Formats { get; init; }

        [JsonPropertyName("description")]
        public string Description { get; init; }

        [JsonPropertyName("upload_date")]
        public string UploadDate { get; init; }

        // public string uploader { get; init; }
        // public string uploader_id { get; init; }
        // public string uploader_url { get; init; }
        // public string channel_id { get; init; }
        // public string channel_url { get; init; }
        [JsonPropertyName("duration")]
        public double DurationInSeconds { get; init; }

        public int view_count { get; init; }
        // public double average_rating { get; init; }
        // public int age_limit { get; init; }

        [JsonPropertyName("webpage_url")]
        public string VideoLink { get; init; }
        // public List<string> categories { get; init; }
        // public List<object> tags { get; init; }
        // public object is_live { get; init; }
        // public int like_count { get; init; }
        // public int dislike_count { get; init; }
        // public object channel { get; init; }
        // public string extractor { get; init; }
        // public string webpage_url_basename { get; init; }
        // public string extractor_key { get; init; }
        // public int n_entries { get; init; }
        // public string playlist { get; init; }

        [JsonPropertyName("playlist_id")]
        public string PlaylistId { get; init; }

        [JsonPropertyName("playlist_title")]
        public string PlaylistTitle { get; init; }

        // public string playlist_uploader { get; init; }
        // public string playlist_uploader_id { get; init; }
        public int Playlist_index { get; init; }

        [JsonPropertyName("thumbnail")]
        public string Thumbnail { get; init; }

        // public string Display_id { get; init; }

        // public int asr { get; init; }
        // public object filesize { get; init; }
        // public string format_id { get; init; }
        // public string format_note { get; init; }

        [JsonPropertyName("fps")]
        public int Fps { get; init; }

        // [JsonPropertyName("height")]
        // public int Height { get; init; }

        [JsonPropertyName("height")]
        public int Quality { get; init; }

        // public double tbr { get; init; }
        // [JsonPropertyName("url")]
        // public string Url { get; init; }

        // public int width { get; init; }
        // public string ext { get; init; }
        // public string vcodec { get; init; }
        // public string acodec { get; init; }
        // public string format { get; init; }
        // public string protocol { get; init; }

        [JsonPropertyName("fulltitle")]
        public string FullTitle { get; init; }

        [JsonPropertyName("_filename")]
        public string FileName { get; init; }

        public DateTime ParsedUploadDate => new DateTime(int.Parse(UploadDate.Substring(0, 4)),
            int.Parse(UploadDate.Substring(4, 2)), int.Parse(UploadDate.Substring(6, 2)));

        public string JsonFile => $"{Title}-{Id}.info.json";
    }
}