using DotCast.Infrastructure.CurrentTenancyProvider;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotCast.Library.Mcp.Tests;

public class LibrarySearchUxTests(McpHostFixture fixture) : IClassFixture<McpHostFixture>
{
    [Fact]
    public async Task Search_matches_titles_without_description_mentions_and_ignores_accents()
    {
        var target = MetadataPatchTests.Book("ux-title", "own");
        target.AudioBookInfo.Name = "UX Marťan";
        target.AudioBookInfo.AuthorName = "Andy Weir";
        var related = MetadataPatchTests.Book("ux-description", "own");
        related.AudioBookInfo.Name = "UX Artemis";
        related.AudioBookInfo.AuthorName = "Andy Weir";
        related.AudioBookInfo.Description = "Also wrote UX Marťan";
        var foreign = MetadataPatchTests.Book("ux-foreign", "foreign");
        foreign.AudioBookInfo.Name = "UX Marťan";
        await using var session = fixture.App.Services.GetRequiredService<IDocumentStore>().LightweightSession(CurrentTenancyProviderNoTenancy.NoTenancyName);
        session.Store(target, related, foreign);
        await session.SaveChangesAsync();
        var results = await fixture.RunAsAsync("owner", messenger => messenger.RequestAsync<AudioBooksRetrievalRequest, IReadOnlyList<AudioBook>>(new(new(SearchText: "UX Martan"))));
        Assert.Equal("ux-title", Assert.Single(results).Id);
        var authors = await fixture.RunAsAsync("owner", messenger => messenger.RequestAsync<AudioBooksRetrievalRequest, IReadOnlyList<AudioBook>>(new(new(SearchText: "Andy Weir"))));
        Assert.Equal(2, authors.Count);
    }

    [Fact]
    public async Task Disconnect_revokes_access_even_when_the_session_has_older_sharing_claims()
    {
        var original = fixture.Owners.Users["owner"].SharedLibraries.ToList();
        try
        {
            var result = await fixture.RunAsAsync("owner", async messenger =>
            {
                // RunAs has already captured the original authentication claims.
                fixture.Owners.Users["owner"].SharedLibraries.Clear();
                return await messenger.RequestAsync<AudioBookDetailRequest, AudioBook?>(new("shared-book"));
            });
            Assert.Null(result);
        }
        finally { fixture.Owners.Users["owner"].SharedLibraries = original; }
    }

    [Fact]
    public async Task Unrated_and_below_sixty_filters_are_distinct()
    {
        var unrated = MetadataPatchTests.Book("ux-unrated", "own"); unrated.Rating = 0; unrated.AudioBookInfo.Name = "ux-unrated";
        var rated = MetadataPatchTests.Book("ux-rated", "own"); rated.Rating = 59; rated.AudioBookInfo.Name = "ux-rated";
        await using var session = fixture.App.Services.GetRequiredService<IDocumentStore>().LightweightSession(CurrentTenancyProviderNoTenancy.NoTenancyName);
        session.Store(unrated, rated); await session.SaveChangesAsync();
        var unratedResults = await fixture.RunAsAsync("owner", messenger => messenger.RequestAsync<AudioBooksRetrievalRequest, IReadOnlyList<AudioBook>>(new(new(SearchText: "ux-", MaxRating: 0))));
        Assert.Contains(unratedResults, book => book.Id == "ux-unrated");
        Assert.DoesNotContain(unratedResults, book => book.Id == "ux-rated");
        var lowResults = await fixture.RunAsAsync("owner", messenger => messenger.RequestAsync<AudioBooksRetrievalRequest, IReadOnlyList<AudioBook>>(new(new(SearchText: "ux-", MinRating: 1, MaxRating: 59))));
        Assert.Contains(lowResults, book => book.Id == "ux-rated");
        Assert.DoesNotContain(lowResults, book => book.Id == "ux-unrated");
    }
}
