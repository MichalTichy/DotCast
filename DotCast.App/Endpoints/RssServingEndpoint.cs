using System.ComponentModel.DataAnnotations;
using Ardalis.ApiEndpoints;
using DotCast.PodcastProvider.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotCast.App.Endpoints
{
    public class RssServingEndpoint : EndpointBaseSync.WithoutRequest.WithActionResult<string>
    {
        private readonly IPodcastFeedProvider feedProvider;

        public RssServingEndpoint(IPodcastFeedProvider feedProvider)
        {
            this.feedProvider = feedProvider;
        }

        [FromRoute]
        [Required]
        public string PodcastName { get; set; } = null!;

        [HttpGet("/podcast/{PodcastName}")]
        [Authorize]
        public override ActionResult<string> Handle()
        {
            var rss = feedProvider.GetRss(PodcastName);


            return new ContentResult
            {
                StatusCode = 200,
                Content = rss,
                ContentType = "application/xml; charset=utf-16"
            };
        }
    }
}
