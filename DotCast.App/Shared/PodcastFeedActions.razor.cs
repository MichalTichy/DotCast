using Microsoft.AspNetCore.Components;

namespace DotCast.App.Shared;

public partial class PodcastFeedActions
{
    private readonly string inputId = $"feed-url-{Guid.NewGuid():N}";

    [Parameter, EditorRequired] public string AudioBookId { get; set; } = "";

    private string FeedEndpoint => $"/api/audiobooks/{Uri.EscapeDataString(AudioBookId)}/podcast-feed";
    private string PlayerEndpoint(PodcastPlayer player) => $"{FeedEndpoint}?player={player}";
}
