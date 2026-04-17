using Ardalis.ApiEndpoints;
using DotCast.Infrastructure.PresignedUrls;
using DotCast.Storage.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DotCast.Storage.API
{
    [AllowAnonymous]
    public class DownloadFileEndpoint(IStorage storage, IPresignedUrlManager presignedUrlManager)
        : EndpointBaseAsync.WithoutRequest.WithActionResult
    {
        [FromRoute(Name = "AudioBookId")]
        public required string AudioBookId { get; set; }

        [FromRoute(Name = "UserId")]
        public required string UserId { get; set; }

        [FromRoute(Name = "FileId")]
        public string? FileId { get; set; }

        [HttpGet("/storage/archive/{AudioBookId}/{UserId}")]
        [HttpGet("/storage/file/{AudioBookId}/{UserId}/{FileId}")]
        public override async Task<ActionResult> HandleAsync(CancellationToken cancellationToken = default)
        {
            var validation = presignedUrlManager.ValidateUrl(HttpContext.Request.GetEncodedUrl());
            if (!validation.result)
            {
                return Unauthorized(validation.message);
            }

            var entry = IsFileRequest()
                ? await storage.GetFileForReadAsync(AudioBookId, FileId!, UserId, cancellationToken)
                : await storage.GetArchiveForReadAsync(AudioBookId, UserId, cancellationToken);
            if (entry == null)
            {
                return NotFound();
            }

            Response.Headers["Cache-Control"] = "public, max-age=31536000";
            Response.Headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");

            return File(entry.Stream, entry.MimeType, Path.GetFileName(entry.Id), true);
        }

        private bool IsFileRequest()
        {
            return FileId != null;
        }
    }
}
