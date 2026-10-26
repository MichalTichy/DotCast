using DotCast.App.Shared;
using DotCast.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotCast.App.API;

[ApiController]
[Authorize]
public class PodcastFeedEndpoint(ILibraryApiInformationProvider libraryApiInfoProvider) : ControllerBase
{
    [HttpGet("/api/audiobooks/{audioBookId}/podcast-feed")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Handle([FromRoute] string audioBookId, [FromQuery] PodcastPlayer? player)
    {
        if (player.HasValue && !Enum.IsDefined(player.Value)) return BadRequest();

        Response.Headers["Referrer-Policy"] = "no-referrer";
        var feedUrl = await libraryApiInfoProvider.GetFeedUrlAsync(audioBookId);
        if (player is null) return Content(feedUrl, "text/plain");

        var feedUri = new Uri(feedUrl);
        var playerUrl = player.Value switch
        {
            PodcastPlayer.Overcast => $"overcast://x-callback-url/add?url={Uri.EscapeDataString(feedUrl)}",
            PodcastPlayer.PocketCasts => $"pktc://subscribe/{feedUri.Authority}{feedUri.PathAndQuery}",
            PodcastPlayer.AntennaPod => $"https://antennapod.org/deeplink/subscribe?url={Uri.EscapeDataString(feedUrl)}",
            _ => throw new ArgumentOutOfRangeException(nameof(player))
        };
        return Redirect(playerUrl);
    }
}
