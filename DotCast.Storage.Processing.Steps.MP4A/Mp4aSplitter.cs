using DotCast.Infrastructure.FileNameNormalization;
using DotCast.Infrastructure.M3U;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using Mp4Chapters;
using Xabe.FFmpeg;

namespace DotCast.Storage.Processing.Steps.MP4A
{
    public class Mp4aSplitter(IFileNameNormalizer fileNameNormalizer, M3uManager m3UManager, ILogger<Mp4aSplitter> logger) : IMp4ASplitter
    {
        public async Task<ICollection<string>> SplitAsync(string source, string destination)
        {
            var chapters = await GetChapters(source);
            var outputFiles = new List<string>();

            foreach (var chapter in chapters)
            {
                var fileName = $"{chapter.Name}.mp3";
                if (!fileNameNormalizer.IsNormalized(fileName))
                {
                    fileName = fileNameNormalizer.Normalize(fileName);
                }

                var outputFilePath = Path.Combine(destination, fileName);
                var startTime = chapter.Time;
                var endTime = chapters.SkipWhile(c => c != chapter).Skip(1).FirstOrDefault()?.Time ?? await GetAudioDuration(source);
                var duration = endTime - startTime;

                await ExtractChapterAsync(source, outputFilePath, startTime, duration);
                outputFiles.Add(outputFilePath);
            }

            var index = Path.Combine(destination, "index.m3u");
            await m3UManager.GenerateM3uFile(outputFiles, index);
            outputFiles.Add(index);

            return outputFiles;
        }

        private async Task<ICollection<Chapter>> GetChapters(string source)
        {
            try
            {
                await using var str = File.OpenRead(source);
                var extractor = new ChapterExtractor(new StreamWrapper(str));
                extractor.Run();

                return extractor.Chapters.Select(t => new Chapter(t.Name, t.Time)).ToArray();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to extract chapters. Keeping as single file.");
                var fileName = Path.GetFileNameWithoutExtension(source);
                var duration = await GetAudioDuration(source);
                return new List<Chapter> { new(fileName, duration) };
            }
        }

        private record Chapter(string Name, TimeSpan Time);

        private async Task ExtractChapterAsync(string source, string output, TimeSpan startTime, TimeSpan duration)
        {
            var conversion = FFmpeg.Conversions.New()
                .AddParameter($"-ss {startTime}")
                .AddParameter($"-t {duration}")
                .AddParameter($"-i \"{source}\"")
                .AddParameter("-q:a 0")
                .AddParameter("-map a")
                .SetOutput(output);

            await conversion.Start();
        }

        private async Task<TimeSpan> GetAudioDuration(string source)
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(source);
            return mediaInfo.Duration;
        }
    }
}