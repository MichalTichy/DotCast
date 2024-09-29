using Ardalis.ApiEndpoints;
using DotCast.SharedKernel.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace DotCast.Library.API
{
    [Authorize]
    public class GetRssEndpoint(IMessageBus messageBus) : EndpointBaseAsync.WithoutRequest.WithActionResult<string>
    {
        [FromRoute]
        public required string AudioBookId { get; set; }

        [HttpGet("/library/{AudioBookId}/rss")]
        public override async Task<ActionResult<string>> HandleAsync(CancellationToken cancellationToken = new())
        {
            var request = new AudioBookRssRequest(AudioBookId);
            var rss = await messageBus.InvokeAsync<string?>(request, cancellationToken);
            if (string.IsNullOrWhiteSpace(rss))
            {
                return NotFound();
            }

            return rss;
        }
    }
}
