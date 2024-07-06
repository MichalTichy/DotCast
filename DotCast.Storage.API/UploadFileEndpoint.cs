using Ardalis.ApiEndpoints;
using DotCast.Infrastructure.PresignedUrls;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace DotCast.Storage.API
{
    [Authorize]
    public class UploadFileEndpoint
        (IStorage storage, IPresignedUrlManager presignedUrlManager, IMessageBus messageBus, ILogger<UploadFileEndpoint> logger) : EndpointBaseAsync.WithRequest<IFormFile>.WithActionResult<string>
    {
        [FromRoute(Name = "AudioBookId")]
        public required string AudioBookId { get; set; }

        [FromRoute(Name = "FileId")]
        public string? FileId { get; set; }

        [HttpPut("/storage/archive/{AudioBookId}/")]
        [HttpPut("/storage/file/{AudioBookId}/{FileId}/")]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [RequestSizeLimit(long.MaxValue)]
        public override async Task<ActionResult<string>> HandleAsync(IFormFile request, CancellationToken cancellationToken = new())
        {
            var requestUrl = ControllerContext.HttpContext.Request.GetEncodedUrl();
            requestUrl = Uri.UnescapeDataString(requestUrl);
            var urlValidationResult = presignedUrlManager.ValidateUrl(requestUrl);
            if (!urlValidationResult.result)
            {
                logger.LogWarning($"Url validation failed {urlValidationResult.message}");
                return Unauthorized(urlValidationResult.message);
            }

            await using var stream = request.OpenReadStream();

            var storageEntry = await storage.StoreAsync(stream, AudioBookId, request.FileName, cancellationToken);
            await messageBus.PublishAsync(new FileUploaded(AudioBookId, request.FileName, Path.GetFileName(storageEntry.LocalPath)));
            return storageEntry.RemotePath;
        }
    }
}