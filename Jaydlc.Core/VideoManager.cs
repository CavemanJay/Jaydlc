using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Jaydlc.Core.Models;

namespace Jaydlc.Core
{
    public class VideoManager //: IVideoManager
    {
        public string RootFolder { get; init; }

        public IEnumerable<VideoInfo>? GetVideos()
        {
            throw new NotImplementedException();
        }

        public async Task<Exception?> DownloadPlaylistInfo(string playlistId)
        {
            try
            {
                await Process.Start("youtube-dl",
                    new[] {"--write-info-json", "--skip-download", playlistId}).WaitForExitAsync();
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        public VideoManager(string rootFolder)
        {
            this.RootFolder = rootFolder;
        }
    }
}