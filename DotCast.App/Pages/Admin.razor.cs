using DotCast.AudioBookProvider.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace DotCast.App.Pages
{
    [Authorize(Roles = "Admin")]
    public partial class Admin
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;

        [Inject]
        public IAudioBookUploader Uploader { get; set; } = null!;

        [Inject]
        public IAudioBookInfoProvider InfoProvider { get; set; } = null!;

        [Inject]
        public IAudioBookDownloader Downloader { get; set; } = null!;

        public List<UploadedFileInfo> Files { get; set; } = new();
        public string AudioBookName { get; set; } = null!;
        public string? ErrorText { get; set; }

        private async Task UploadFiles(InputFileChangeEventArgs e)
        {
            ErrorText = null;

            Files = e.GetMultipleFiles(int.MaxValue).Select(t => new UploadedFileInfo(t)).ToList();

            if (!Files.Any())
            {
                return;
            }

            if (Files.Any(t => t.IsZip))
            {
                if (Files.Count != 1)
                {
                    ErrorText = "When uploading as zip than only single file is allowed.";
                    return;
                }

                await UploadAudioBookZip(Files.Single());
            }
            else
            {
                await UploadAudioBookFiles(Files);
            }
        }

        private async Task UploadAudioBookZip(UploadedFileInfo zip)
        {
            var file = zip.File;

            string? AudioBookId;
            await using (var writeStream = Uploader.GetAudioBookZipWriteStream(AudioBookName, out AudioBookId))
            {
                await using var readStream = file.OpenReadStream(int.MaxValue);
                var bytesRead = 0;
                double totalRead = 0;
                var buffer = new byte[1024 * 10];

                while ((bytesRead = await readStream.ReadAsync(buffer)) != 0)
                {
                    totalRead += bytesRead;

                    await writeStream.WriteAsync(buffer, 0, bytesRead);

                    zip.UploadPercentage = (int) (totalRead / file.Size * 100);

                    StateHasChanged();
                }
            }

            _ = Uploader.UnzipAudioBook(AudioBookId);
        }

        private async Task UploadAudioBookFiles(List<UploadedFileInfo> files)
        {
            var id = string.Empty;
            foreach (var fileInfo in files)
            {
                var file = fileInfo.File;

                await using var writeStream = Uploader.GetAudioBookFileWriteStream(AudioBookName, fileInfo.File.Name, fileInfo.File.ContentType, out id);
                await using var readStream = file.OpenReadStream(int.MaxValue);
                var bytesRead = 0;
                double totalRead = 0;
                var buffer = new byte[1024 * 10];

                while ((bytesRead = await readStream.ReadAsync(buffer)) != 0)
                {
                    totalRead += bytesRead;

                    await writeStream.WriteAsync(buffer, 0, bytesRead);

                    fileInfo.UploadPercentage = (int) (totalRead / file.Size * 100);

                    StateHasChanged();
                }
            }

            _ = Downloader.GenerateZip(id, true);
        }

        private void UnzipAll()
        {
            _ = Task.Run(async () =>
            {
                foreach (var AudioBookId in Downloader.GetAudioBookIdsAvailableForDownload())
                {
                    Console.WriteLine($"Unziping {AudioBookId}");
                    await Uploader.UnzipAudioBook(AudioBookId);
                }

                Console.WriteLine("All zips unziped");
            });
        }

        private void ZipAll()
        {
            _ = Task.Run(async () =>
            {
                var AudioBookInfos = InfoProvider.GetAudioBooks();

                await foreach (var AudioBookInfo in AudioBookInfos)
                {
                    Console.WriteLine($"Generating zip for {AudioBookInfo.Name}");
                    await Downloader.GenerateZip(AudioBookInfo.Id);
                }

                Console.WriteLine("All zips generated");
            });
        }
    }

    public record UploadedFileInfo(IBrowserFile File)
    {
        public bool IsZip => File.ContentType.ToLower().Contains("zip");
        public int UploadPercentage { get; set; }
    }
}
