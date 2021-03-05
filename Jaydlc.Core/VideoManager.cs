using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        /// Uses youtube-dl executable to download the information about videos in a playlist.
        /// Writes output to log files within the <see cref="logRoot"/> folder.
        /// </summary>
        /// <param name="logRoot">The root folder to place youtube-dl output</param>
        /// <exception cref="ExeNotFoundException">Youtube-dl executable is not found in path</exception>
        public async Task DownloadPlaylistInfo(string logRoot)
        {
            _ = logRoot ?? throw new ArgumentNullException(nameof(logRoot));

            // Create the folder if it does not exist 
            _ = Directory.Exists(logRoot) ? null : Directory.CreateDirectory(logRoot);

            var outString = Path.Join(this.RootFolder, "%(title)s-%(id)s.%(ext)s");
            var ytdlArgs = new[]
            {
                "-o", outString, "--write-info-json",
                "--skip-download",
                this.PlaylistId
            };

            try
            {
                var startInfo = new ProcessStartInfo("youtube-dl")
                {
                    Arguments = string.Join(' ', ytdlArgs),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                var process = new Process() { StartInfo = startInfo };

                var datePrefix = DateTime.Now.ToShortDateString().Replace("/", "_").Replace(@"\", "_");
                process.OutputDataReceived += (sender, dataReceivedEventArgs) =>
                {
                    var content = dataReceivedEventArgs.Data;
                    if (content is null)
                    {
                        return;
                    }

                    using var outFile = new FileStream(Path.Join(logRoot, $"{datePrefix}-youtubedl.log"),
                        FileMode.Append);

                    content = $"{DateTime.Now.ToLongTimeString()}: {content}\n";
                    var dataToWrite = Encoding.UTF8.GetBytes(content);
                    outFile.Write(dataToWrite);
                };

                process.ErrorDataReceived += (sender, dataReceivedEventArgs) =>
                {
                    var content = dataReceivedEventArgs.Data;
                    if (content is null)
                    {
                        return;
                    }

                    using var errorFile = new FileStream(Path.Join(logRoot, $"{datePrefix}-error.log"),
                        FileMode.Append);

                    content = $"{DateTime.Now.ToLongTimeString()}: {content}\n";

                    if (content.Contains("Finished downloading playlist"))
                        content += "\n";

                    var dataToWrite = Encoding.UTF8.GetBytes(content);
                    errorFile.Write(dataToWrite);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

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