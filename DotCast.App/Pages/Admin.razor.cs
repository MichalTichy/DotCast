using DotCast.PodcastProvider.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace DotCast.App.Pages
{
    public partial class Admin
    {
        [Inject]
        public IPodcastUploader Uploader { get; set; } = null!;

        public List<UploadedFileInfo> Files { get; set; } = new();
        public string PodcastName { get; set; }

        private async Task UploadFiles(InputFileChangeEventArgs e)
        {
            Files = e.GetMultipleFiles(int.MaxValue).Select(t => new UploadedFileInfo(t)).ToList();

            foreach (var fileInfo in Files)
            {
                var file = fileInfo.File;

                await using var writeStream = Uploader.GetWriteStream(PodcastName, fileInfo.File.Name, fileInfo.File.ContentType);
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
        }

        private static string GetFilePath(UploadedFileInfo fileInfo)
        {
            var trustedFileName = Path.GetRandomFileName();
            var path = Path.Combine(@"C:\TMP\tst", trustedFileName);
            return path;
        }
    }

    public record UploadedFileInfo(IBrowserFile File)
    {
        public int UploadPercentage { get; set; }
    }
}
