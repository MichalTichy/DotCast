using Ardalis.ApiEndpoints;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.PresignedUrls;
using DotCast.SharedKernel.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace DotCast.Library.API
{
    public class GetRssEndpoint(IMessagePublisher messenger, IPresignedUrlManager presignedUrlManager) : EndpointBaseAsync.WithoutRequest.WithActionResult<string>
    {
        [FromRoute]
        public required string AudioBookId { get; set; }

        [FromRoute]
        public required string UserId { get; set; }

        [FromRoute]
        public required string Signature { get; set; }

        [HttpGet("/library/{AudioBookId}/{UserId}/rss/{Signature}")]
        public override async Task<ActionResult<string>> HandleAsync(CancellationToken cancellationToken = new())
        {
            var validation = presignedUrlManager.ValidateUrl(HttpContext.Request.GetEncodedUrl());
            if (!validation.result)
            {
                return Unauthorized(validation.message);
            }

            var request = new AudioBookRssRequest(AudioBookId);
            var rss = await messenger.RequestAsync<AudioBookRssRequest, string?>(request, cancellationToken);
            if (string.IsNullOrWhiteSpace(rss))
            {
                return NotFound();
            }

            return rss;
        }
    }
}
