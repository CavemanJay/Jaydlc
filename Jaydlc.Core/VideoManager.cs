using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Jaydlc.Core.Models;

namespace Jaydlc.Core
{
    public class VideoManager : IVideoManager
    {
        public string RootFolder { get; init; }
        public string PlaylistId { get; }

        public IEnumerable<VideoInfo>? GetVideos()
        {
            throw new NotImplementedException();
        }

        public async Task<Exception?> DownloadPlaylistInfo()
        {
            try
            {
                await Process.Start("youtube-dl",
                        new[] {"--write-info-json", "--skip-download", this.PlaylistId})
                    .WaitForExitAsync();
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        public VideoManager(string rootFolder, string playlistId)
        {
            this.RootFolder = rootFolder;
            PlaylistId = playlistId;
        }
    }
}