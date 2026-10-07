using System.Text.Json;
using DotCast.Library.Mcp.Models;
using DotCast.SharedKernel.Models;
using Xunit;
namespace DotCast.Library.Mcp.Tests;
public sealed class MetadataPatchTests
{
    [Fact]
    public void PatchPreservesOmittedFieldsAndStorage()
    {
        var book = Book();
        Patch("""{"title":"New title","description":null,"categories":[]}""").Apply(book);
        Assert.Equal("New title", book.AudioBookInfo.Name);
        Assert.Equal("Author", book.AudioBookInfo.AuthorName);
        Assert.Null(book.AudioBookInfo.Description);
        Assert.Empty(book.AudioBookInfo.Categories);
        Assert.Equal("own", book.LibraryId);
        Assert.Equal("file", Assert.Single(book.AudioBookInfo.Chapters).FileId);
        Assert.True(book.AudioBookInfo.HasArchive);
    }
    [Theory]
    [InlineData("""{"LibraryId":"foreign"}""")] [InlineData("""{"chapters":[]}""")]
    [InlineData("""{"rating":101}""")] [InlineData("""{"positionInSeries":-1}""")]
    [InlineData("""{"title":null}""")] [InlineData("""{"author":" "}""")]
    [InlineData("""{"releaseDate":"2026-02-30"}""")] [InlineData("""{"categories":["unknown"]}""")]
    [InlineData("""{"imageUrl":"javascript:alert(1)"}""")] [InlineData("{}")]
    public void InvalidPatchDoesNotMutateBook(string json)
    {
        var book = Book();
        Assert.Throws<ArgumentException>(() => Patch(json).Apply(book));
        Assert.Equal("Title", book.AudioBookInfo.Name);
        Assert.Equal("own", book.LibraryId);
    }
    internal static AudioBookMetadataPatch Patch(string json) => new(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!);
    internal static AudioBook Book(string id = "book", string library = "own") => new() {
        Id = id, LibraryId = library, Rating = 50,
        AudioBookInfo = new() { Id = id, Name = "Title", AuthorName = "Author", Description = "Description",
            Chapters = [new() { Name = "Chapter", FileId = "file", DurationInMinutes = 10 }], HasArchive = true } };
}
