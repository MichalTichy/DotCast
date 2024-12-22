using Ardalis.ApiEndpoints;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.PresignedUrls;
using DotCast.SharedKernel.Messages;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.Blazor.ClaimsManagement;
using Shared.Infrastructure.UserManagement.Abstractions;
using Wolverine;

namespace DotCast.Library.API
{
    public class GetRssEndpoint(IMessagePublisher messenger, IPresignedUrlManager presignedUrlManager, IUserClaimsProvider claimsService, IUserManager<UserInfo> userManager)
        : EndpointBaseAsync.WithoutRequest.WithActionResult<string>
    {
        [FromRoute]
        public required string AudioBookId { get; set; }

        [FromRoute]
        public required string UserId { get; set; }

        [HttpGet("/library/{AudioBookId}/{UserId}/rss")]
        public override async Task<ActionResult<string>> HandleAsync(CancellationToken cancellationToken = new())
        {
            var validation = presignedUrlManager.ValidateUrl(HttpContext.Request.GetEncodedUrl());
            if (!validation.result)
            {
                return Unauthorized(validation.message);
            }

            var user = await userManager.GetUserAsync(UserId);
            claimsService.User = user?.GetClaimsIdentity();

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
