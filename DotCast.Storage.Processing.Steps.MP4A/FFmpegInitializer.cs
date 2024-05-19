using DotCast.Infrastructure.Initializer;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace DotCast.Storage.Processing.Steps.MP4A
{
    public class FFmpegInitializer : InitializerBase
    {
        public override int Priority => 0;

        protected override async Task RunInitializationLogicAsync()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "ffmpeg");
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, path);
            FFmpeg.SetExecutablesPath(path);
        }
    }
}