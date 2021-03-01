using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Jaydlc.Core.Exceptions;
using Jaydlc.Core.Models;

namespace Jaydlc.Core
{
    public class VideoManager
    {
        public string RootFolder { get; init; }
        public string PlaylistId { get; }

        public IEnumerable<VideoInfo>? GetVideos()
        {
            foreach (var jsonFile in Directory.GetFiles(RootFolder)
                .Where(x => x.EndsWith("info.json")))
            {
                if (jsonFile is null) continue;

                var info = JsonSerializer.Deserialize<VideoInfo>(File.ReadAllText(jsonFile));

                if (info is null) continue;
                yield return info;
            }
        }

        /// <summary>
        /// Uses youtube-dl executable to download the information about videos in a playlist
        /// </summary>
        /// <exception cref="ExeNotFoundException">Youtube-dl executable is not found in path</exception>
        public async Task DownloadPlaylistInfo(Action<string?>? stdOutHandler = null)
        {
            try
            {
                var process = Process.Start("youtube-dl", new[]
                {
                    "-o", $"{this.RootFolder}/%(title)s-%(id)s.%(ext)s", "--write-info-json",
                    "--skip-download",
                    this.PlaylistId
                });

                if (stdOutHandler is not null)
                {
                    process.OutputDataReceived += (sender, args) => { stdOutHandler(args.Data); };
                }

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