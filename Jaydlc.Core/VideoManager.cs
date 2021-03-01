using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Jaydlc.Core.Exceptions;
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

        /// <summary>
        /// Uses youtube-dl executable to download the information about videos in a playlist
        /// </summary>
        /// <exception cref="Exception">Fuck</exception>
        public async Task DownloadPlaylistInfo()
        {
            try
            {
                var process = Process.Start("youtube-dl", new[]
                {
                    "-o", $"{this.RootFolder}/$(title)s-%(id)s.%(ext)s", "--write-info-json",
                    "--skip-download",
                    this.PlaylistId
                });

                process.OutputDataReceived += (sender, args) => { Console.WriteLine(args.Data); };

                await process.WaitForExitAsync();
            }
            catch (Win32Exception ex)
            {
                if (ex.Message.Contains("No such file"))
                {
                    throw new ExeNotFoundException("youtube-dl");
                }

                throw;
            }
        }

        public VideoManager(string rootFolder, string playlistId)
        {
            this.RootFolder = rootFolder;
            PlaylistId = playlistId;
        }
    }
}