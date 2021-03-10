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
        private readonly string _logRoot;
        private readonly FileSystemWatcher _watcher;
        private bool _running = false;

        public string RootFolder { get; init; }
        public string PlaylistId { get; }

        public ObservableCollection<VideoInfo> Videos { get; init; } = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="playlistId"></param>
        /// <param name="logRoot">The root folder to place youtube-dl output</param>
        public VideoManager(string rootFolder, string playlistId,
            string logRoot)
        {
            _ = rootFolder ??
                throw new ArgumentNullException(nameof(rootFolder));
            _ = playlistId ??
                throw new ArgumentNullException(nameof(playlistId));
            this._logRoot = logRoot ??
                            throw new ArgumentNullException(nameof(logRoot));

            this.RootFolder = Path.GetFullPath(rootFolder);
            this.PlaylistId = playlistId;

            this.initVideos();

            this._watcher = new FileSystemWatcher(this.RootFolder);
            this._watcher.Created += this.VideoInfoDownloaded;
            this._watcher.Deleted += this.VideoInfoDeleted;
            this._watcher.Renamed += this.VideoInfoDownloaded;

            this._watcher.EnableRaisingEvents = true;
        }

        private void initVideos()
        {
            foreach (var jsonFile in Directory.GetFiles(this.RootFolder)
                                              .Where(
                                                  x => x.EndsWith("info.json")
                                              ))
            {
                if (jsonFile is null) continue;

                var info =
                    JsonSerializer.Deserialize<VideoInfo>(
                        File.ReadAllText(jsonFile)
                    );

                if (info is null) continue;
                if (this.Videos.Contains(info)) continue;

                this.Videos.Add(info);
            }
        }

        private static void LogToFile(string root, string fileName,
            DataReceivedEventArgs dataReceivedEventArgs)
        {
            var datePrefix = DateTime.Now.ToShortDateString()
                                     .Replace("/", "_")
                                     .Replace(@"\", "_");

            var content = dataReceivedEventArgs.Data;
            if (content is null)
            {
                return;
            }

            using var outFile = new FileStream(
                Path.Join(root, $"{datePrefix}-{fileName}"), FileMode.Append
            );

            content = $"{DateTime.Now.ToLongTimeString()}: {content}\n";
            if (content.Contains("Finished downloading playlist"))
                content += "\n";

            var dataToWrite = Encoding.UTF8.GetBytes(content);
            outFile.Write(dataToWrite);
        }

        /// <summary>
        /// Uses youtube-dl executable to download the information about videos in a playlist.
        /// Writes output to log files within the <see cref="logRoot"/> folder.
        /// </summary>
        /// <exception cref="ExeNotFoundException">Youtube-dl executable is not found in path</exception>
        public async Task DownloadPlaylistInfo()
        {
            if (this._running) return;

            this._running = true;

            // Create the folder if it does not exist 
            _ = Directory.Exists(this._logRoot)
                ? null
                : Directory.CreateDirectory(this._logRoot);

            var outString = Path.Join(
                this.RootFolder, "%(title)s-%(id)s.%(ext)s"
            );
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

                var process = new Process {StartInfo = startInfo};

                process.OutputDataReceived += (sender, data) =>
                {
                    LogToFile(this._logRoot, "youtubedl.log", data);
                };

                process.ErrorDataReceived += (sender, data) =>
                {
                    LogToFile(this._logRoot, "error.log", data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();
            }
            catch (Win32Exception ex) when (ex.Message.Contains("No such file"))
            {
                this._running = false;
                throw new ExeNotFoundException("youtube-dl", ex);
            }
            catch (Exception)
            {
                this._running = false;
                throw;
            }
        }


        private void VideoInfoDeleted(object sender, FileSystemEventArgs e)
        {
            lock (this.Videos)
            {
                var deletedVid =
                    this.Videos.FirstOrDefault(x => x.JsonFile == e.Name);
                if (deletedVid is null) return;

                this.Videos.Remove(deletedVid);
            }
        }


        private async void VideoInfoDownloaded(object sender,
            FileSystemEventArgs e)
        {
            // Wait to make sure the file is fully written
            await Task.Delay(1000);
            if (!e.Name.EndsWith("info.json"))
            {
                return;
            }

            var info = JsonSerializer.Deserialize<VideoInfo>(
                await File.ReadAllTextAsync(e.FullPath)
            );
            if (info is null) return;

            lock (this.Videos)
            {
                var existing = this.Videos.FirstOrDefault(x => x.Id == info.Id);

                // Add the video to the list if it doesn't exist yet
                if (existing is null)
                {
                    this.Videos.Add(info);
                    return;
                }

                // Update the video if it exists already
                var index = this.Videos.IndexOf(existing);
                this.Videos[index] = info;
            }
        }

        public void Dispose()
        {
            this._watcher.Created -= this.VideoInfoDownloaded;
            this._watcher.Deleted -= this.VideoInfoDeleted;
            this._watcher.Renamed -= this.VideoInfoDownloaded;
            this._watcher.Dispose();
        }
    }
}