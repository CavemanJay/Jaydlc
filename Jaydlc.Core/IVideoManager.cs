using System;
using System.Collections.Generic;
using Jaydlc.Core.Models;

namespace Jaydlc.Core
{
    public interface IVideoManager
    {
        public string RootFolder { get; }

        IEnumerable<VideoInfo>? GetVideos();
        Exception? DownloadPlaylistInfo(string playlistId);
    }
}