using DotCast.Infrastructure.FileNameNormalization;
using DotCast.Infrastructure.M3U;
using DotCast.Storage.Abstractions;
using Mp4Chapters;
using Xabe.FFmpeg;

namespace DotCast.Storage.Processing.Steps.MP4A
{
    public class Mp4aSplitter(IFileNameNormalizer fileNameNormalizer, M3uManager m3UManager) : IMp4ASplitter
    {
        public async Task<ICollection<string>> SplitAsync(string source, string destination)
        {
            var chapters = GetChapters(source);
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

        private ICollection<Chapter> GetChapters(string source)
        {
            using var str = File.OpenRead(source);
            var extractor = new ChapterExtractor(new StreamWrapper(str));
            extractor.Run();

            return extractor.Chapters.Select(t => new Chapter(t.Name, t.Time)).ToArray();
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