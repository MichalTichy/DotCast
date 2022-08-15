using DotCast.PodcastProvider.Base;
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
        public IPodcastUploader Uploader { get; set; } = null!;

        [Inject]
        public IPodcastInfoProvider InfoProvider { get; set; } = null!;

        [Inject]
        public IPodcastDownloader Downloader { get; set; } = null!;

        public List<UploadedFileInfo> Files { get; set; } = new();
        public string PodcastName { get; set; } = null!;
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

                await UploadPodcastZip(Files.Single());
            }
            else
            {
                await UploadPodcastFiles(Files);
            }
        }

        private async Task UploadPodcastZip(UploadedFileInfo zip)
        {
            var file = zip.File;

            string? podcastId;
            await using (var writeStream = Uploader.GetPodcastZipWriteStream(PodcastName, out podcastId))
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

            _ = Uploader.UnzipPodcast(podcastId);
        }

        private async Task UploadPodcastFiles(List<UploadedFileInfo> files)
        {
            var id = string.Empty;
            foreach (var fileInfo in files)
            {
                var file = fileInfo.File;

                await using var writeStream = Uploader.GetPodcastFileWriteStream(PodcastName, fileInfo.File.Name, fileInfo.File.ContentType, out id);
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
                foreach (var podcastInfo in InfoProvider.GetPodcasts())
                {
                    Console.WriteLine($"Unziping {podcastInfo.Name}");
                    await Uploader.UnzipPodcast(podcastInfo.Id);
                }

                Console.WriteLine("All zips unziped");
            });
        }

        private void ZipAll()
        {
            _ = Task.Run(async () =>
            {
                foreach (var podcastInfo in InfoProvider.GetPodcasts())
                {
                    Console.WriteLine($"Generating zip for {podcastInfo.Name}");
                    await Downloader.GenerateZip(podcastInfo.Id);
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
