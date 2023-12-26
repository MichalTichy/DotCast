using Ardalis.ApiEndpoints;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotCast.Storage.API
{
    [Authorize]
    public class DownloadFileEndpoint(IStorage storage) : EndpointBaseSync.WithoutRequest.WithActionResult
    {
        [FromRoute(Name = "AudioBookId")]
        public required string AudioBookId { get; set; }

        [FromRoute(Name = "FileId")]
        public string? FileId { get; set; }

        [HttpGet("/storage/archive/{AudioBookId}")]
        [HttpGet("/storage/file/{AudioBookId}/{FileId}")]
        public override ActionResult Handle()
        {
            var entry = IsFileRequest() ? storage.GetFileForRead(AudioBookId, FileId!) : storage.GetArchiveForRead(AudioBookId);

            if (entry == null)
            {
                return NotFound();
            }

            return File(entry.Stream, entry.MimeType, entry.Id, true);
        }

        private bool IsFileRequest()
        {
            return FileId != null;
        }
    }
}