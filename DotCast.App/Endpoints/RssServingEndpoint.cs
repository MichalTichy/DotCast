using System.ComponentModel.DataAnnotations;
using Ardalis.ApiEndpoints;
using DotCast.AudioBookProvider.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotCast.App.Endpoints
{
    public class RssServingEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<string>
    {
        private readonly IAudioBookFeedProvider feedProvider;

        public RssServingEndpoint(IAudioBookFeedProvider feedProvider)
        {
            this.feedProvider = feedProvider;
        }

        [FromRoute]
        [Required]
        public string AudioBookName { get; set; } = null!;

        [HttpGet("/AudioBook/{AudioBookName}")]
        [Authorize]
        public override async Task<ActionResult<string>> HandleAsync(CancellationToken cancellationToken = new())
        {
            var rss = await feedProvider.GetRss(AudioBookName);


            return new ContentResult
            {
                StatusCode = 200,
                Content = rss,
                ContentType = "application/xml; charset=utf-16"
            };
        }
    }
}
