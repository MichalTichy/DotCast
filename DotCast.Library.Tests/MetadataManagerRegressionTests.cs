using System.Text;
using DotCast.Infrastructure.M3U;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using AudioMetadataManager = DotCast.Infrastructure.MetadataManager.MetadataManager;

namespace DotCast.Library.Tests;

public sealed class MetadataManagerRegressionTests : IDisposable
{
    private readonly string directory = Directory.CreateTempSubdirectory("dotcast-metadata-").FullName;
    private readonly AudioMetadataManager manager = new(new M3uManager(), NullLogger<AudioMetadataManager>.Instance);

    [Theory]
    [InlineData("cover.jpg")]
    [InlineData("cover.JPG")]
    [InlineData("cover.jpeg")]
    [InlineData("cover.png")]
    [InlineData("cover.gif")]
    [InlineData("cover.bmp")]
    [InlineData("cover.tiff")]
    public async Task Invalid_cover_does_not_block_audio_tags_or_chapter_order_from_being_saved(string coverName)
    {
        var coverPath = Path.Combine(directory, coverName);
        byte[] invalidCover = [0, 1, 2, 3];
        await File.WriteAllBytesAsync(coverPath, invalidCover);
        var first = CreateWaveFile("first.wav");
        var second = CreateWaveFile("second.wav");
        var playlistPath = Path.Combine(directory, "index.m3u");
        await File.WriteAllTextAsync(playlistPath, "#EXTM3U\nfirst.wav\nsecond.wav\n");
        var book = Book("second.wav", "first.wav");
        var source = new StorageEntryWithFiles(book.Id,
            [LocalFile(coverPath), LocalFile(first), LocalFile(second), LocalFile(playlistPath)], null);

        await manager.UpdateMetadata(book, source);

        Assert.Equal(invalidCover, await File.ReadAllBytesAsync(coverPath));
        Assert.Equal(["#EXTM3U", "second.wav", "first.wav"], await File.ReadAllLinesAsync(playlistPath));
        for (var index = 0; index < book.Chapters.Count; index++)
        {
            using var audio = TagLib.File.Create(Path.Combine(directory, book.Chapters[index].FileId));
            Assert.Equal(book.Name, audio.Tag.Album);
            Assert.Equal([book.AuthorName], audio.Tag.Performers);
            Assert.Equal(book.Chapters[index].Name, audio.Tag.Title);
            Assert.Equal((uint)(index + 1), audio.Tag.Track);
        }
    }

    [Fact]
    public async Task Corrupt_audio_still_fails_the_save_without_replacing_the_playlist()
    {
        var audioPath = Path.Combine(directory, "broken.mp3");
        await File.WriteAllBytesAsync(audioPath, [0, 1, 2, 3]);
        var playlistPath = Path.Combine(directory, "index.m3u");
        const string originalPlaylist = "#EXTM3U\nbroken.mp3\n";
        await File.WriteAllTextAsync(playlistPath, originalPlaylist);
        var book = Book("broken.mp3");
        var source = new StorageEntryWithFiles(book.Id, [LocalFile(audioPath), LocalFile(playlistPath)], null);

        var error = await Assert.ThrowsAsync<AggregateException>(() => manager.UpdateMetadata(book, source));

        Assert.Contains(error.InnerExceptions, exception => exception is TagLib.CorruptFileException);
        Assert.Equal(originalPlaylist, await File.ReadAllTextAsync(playlistPath));
    }

    private static AudioBookInfo Book(params string[] files) => new()
    {
        Id = "test-save",
        Name = "Updated title",
        AuthorName = "Updated author",
        Chapters = files.Select(file => new Chapter
        {
            FileId = file,
            Name = $"Chapter {file}",
            DurationInMinutes = 1
        }).ToList()
    };

    private static LocalFileInfo LocalFile(string path) => new(path, $"/storage/{Path.GetFileName(path)}");

    private string CreateWaveFile(string name)
    {
        var path = Path.Combine(directory, name);
        using var writer = new BinaryWriter(File.Create(path));
        const int sampleRate = 8000;
        const int dataLength = sampleRate * sizeof(short);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataLength);
        writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(sampleRate);
        writer.Write(sampleRate * sizeof(short));
        writer.Write((short)sizeof(short));
        writer.Write((short)16);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataLength);
        writer.Write(new byte[dataLength]);
        return path;
    }

    public void Dispose() => Directory.Delete(directory, recursive: true);
}
