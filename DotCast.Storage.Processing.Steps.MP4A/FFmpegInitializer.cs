using Shared.Infrastructure.Initializer;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace DotCast.Storage.Processing.Steps.MP4A
{
    public class FFmpegInitializer : InitializerBase
    {
        public override int Priority => 0;
        public override InitializerTrigger Trigger => InitializerTrigger.OnStartup;
        public override bool RunOnlyInLeaderInstance => false;

        protected override async Task RunInitializationLogicAsync()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "ffmpeg");
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, path);
            FFmpeg.SetExecutablesPath(path);
        }
    }
}