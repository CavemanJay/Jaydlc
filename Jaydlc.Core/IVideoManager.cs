using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jaydlc.Core.Models;

namespace Jaydlc.Core
{
    public interface IVideoManager
    {
        public string RootFolder { get; }

        IEnumerable<VideoInfo>? GetVideos();
        Task<Exception?> DownloadPlaylistInfo(string playlistId);
    }
}