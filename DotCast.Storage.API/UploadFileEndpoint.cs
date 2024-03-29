﻿using Ardalis.ApiEndpoints;
using DotCast.Infrastructure.PresignedUrls;
using DotCast.SharedKernel.Messages;
using DotCast.Storage.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace DotCast.Storage.API
{
    [Authorize]
    public class UploadFileEndpoint(IStorage storage, IPresignedUrlManager presignedUrlManager, IMessageBus messageBus) : EndpointBaseAsync.WithRequest<IFormFile>.WithActionResult<string>
    {
        [FromRoute(Name = "AudioBookId")]
        public required string AudioBookId { get; set; }

        [FromRoute(Name = "FileId")]
        public string? FileId { get; set; }

        [HttpPut("/storage/archive/{AudioBookId}/")]
        [HttpPut("/storage/file/{AudioBookId}/{FileId}/")]
        [RequestFormLimits(MultipartBodyLengthLimit = 2000000000)]
        [RequestSizeLimit(20000000000)]
        public override async Task<ActionResult<string>> HandleAsync(IFormFile request, CancellationToken cancellationToken = new())
        {
            var requestUrl = ControllerContext.HttpContext.Request.GetEncodedUrl();
            if (!presignedUrlManager.ValidateUrl(requestUrl))
            {
                return Unauthorized();
            }

            await using var stream = request.OpenReadStream();
            var storageEntry = await storage.StoreAsync(stream, AudioBookId, request.FileName, cancellationToken);
            await messageBus.PublishAsync(new FileUploaded(AudioBookId, request.FileName));
            return storageEntry.RemotePath;
        }
    }
}