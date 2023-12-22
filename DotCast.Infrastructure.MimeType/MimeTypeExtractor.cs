namespace DotCast.Infrastructure.MimeType
{
    public static class MimeTypeExtractor
    {
        public static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                // Image types
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" or ".tif" => "image/tiff",
                ".svg" => "image/svg+xml",

                // Audio types
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".ogg" => "audio/ogg",
                ".aac" => "audio/aac",
                ".flac" => "audio/flac",
                ".m4a" => "audio/mp4",

                // Archive types
                ".zip" => "application/zip",
                ".rar" => "application/vnd.rar",
                ".7z" => "application/x-7z-compressed",
                ".tar" => "application/x-tar",
                ".gz" => "application/gzip",
                ".bz2" => "application/x-bzip2",

                // Fallback for unknown types
                _ => "application/octet-stream"
            };
        }
    }
}
