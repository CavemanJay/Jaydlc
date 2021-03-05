using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Jaydlc.Core.Exceptions;
using Jaydlc.Core.Models;

namespace Jaydlc.Core
{
    public class VideoManager : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        public string RootFolder { get; init; }
        public string PlaylistId { get; }

        public ObservableCollection<VideoInfo> Videos { get; init; } = new();

        private void initVideos()
        {
            foreach (var jsonFile in Directory.GetFiles(RootFolder)
                .Where(x => x.EndsWith("info.json")))
            {
                if (jsonFile is null) continue;

                var info = JsonSerializer.Deserialize<VideoInfo>(File.ReadAllText(jsonFile));

                if (info is null) continue;
                if (Videos.Contains(info)) continue;

                Videos.Add(info);
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
            _ = rootFolder ?? throw new ArgumentNullException(nameof(rootFolder));
            _ = playlistId ?? throw new ArgumentNullException(nameof(playlistId));

            this.RootFolder = Path.GetFullPath(rootFolder);
            PlaylistId = playlistId;

            initVideos();

            _watcher = new FileSystemWatcher(RootFolder);
            _watcher.Created += VideoInfoDownloaded;
            _watcher.Deleted += VideoInfoDeleted;
            _watcher.Renamed += VideoInfoDownloaded;

            _watcher.EnableRaisingEvents = true;
        }

        private void VideoInfoDeleted(object sender, FileSystemEventArgs e)
        {
            lock (Videos)
            {
                var deletedVid = Videos.FirstOrDefault(x => x.JsonFile == e.Name);
                if (deletedVid is null) return;

                Videos.Remove(deletedVid);
            }
        }


        private async void VideoInfoDownloaded(object sender, FileSystemEventArgs e)
        {
            // Wait to make sure the file is fully written
            await Task.Delay(1000);
            if (!e.Name.EndsWith("info.json"))
            {
                return;
            }

            var info =
                JsonSerializer.Deserialize<VideoInfo>(await File.ReadAllTextAsync(e.FullPath));
            if (info is null) return;

            lock (Videos)
            {
                if (Videos.Contains(info)) return;

                Videos.Add(info);
            }
        }

        public void Dispose()
        {
            _watcher.Created -= VideoInfoDownloaded;
            _watcher.Deleted -= VideoInfoDeleted;
            _watcher.Renamed -= VideoInfoDownloaded;
            _watcher.Dispose();
        }
    }
}