using Ardalis.ApiEndpoints;
using DotCast.Infrastructure.PresignedUrls;
using DotCast.Storage.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DotCast.Storage.API
{
    [Authorize]
    public class DownloadFileEndpoint(IStorage storage, IPresignedUrlManager presignedUrlManager) : EndpointBaseSync.WithoutRequest.WithActionResult
    {
        [FromRoute(Name = "AudioBookId")]
        public required string AudioBookId { get; set; }

        [FromRoute(Name = "FileId")]
        public string? FileId { get; set; }

        [HttpGet("/storage/archive/{AudioBookId}")]
        [HttpGet("/storage/file/{AudioBookId}/{FileId}")]
        public override ActionResult Handle()
        {
            var validation = presignedUrlManager.ValidateUrl(HttpContext.Request.GetEncodedUrl());
            if (!validation.result)
            {
                return Unauthorized(validation.message);
            }

            var entry = IsFileRequest() ? storage.GetFileForRead(AudioBookId, FileId!) : storage.GetArchiveForRead(AudioBookId);

            if (entry == null)
            {
                return NotFound();
            }

            Response.Headers["Cache-Control"] = "public, max-age=31536000"; // Cache for 1 year
            Response.Headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R"); // Expiry date for the cache

            return File(entry.Stream, entry.MimeType, entry.Id, true);
        }

        private bool IsFileRequest()
        {
            return FileId != null;
        }
    }
}