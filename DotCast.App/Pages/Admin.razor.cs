using Blazorise;
using DotCast.AudioBookProvider.Base;
using DotCast.AudioBookProvider.FileSystem;
using DotCast.Infrastructure.BookInfoProvider.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;

namespace DotCast.App.Pages
{
    [Authorize(Roles = "Admin")]
    public partial class Admin
    {
        private Modal uploadModalRef = null!;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; } = null!;

        [Inject]
        public IAudioBookUploader Uploader { get; set; } = null!;

        [Inject]
        public IBookInfoProvider BookInfoProvider { get; set; } = null!;

        [Inject]
        public IAudioBookInfoProvider InfoProvider { get; set; } = null!;

        [Inject]
        public FileSystemAudioBookProvider FileSystemInfoProvider { get; set; } = null!;

        [Inject]
        public IAudioBookDownloader Downloader { get; set; } = null!;

        public List<UploadedFileInfo> Files { get; set; } = new();
        public string AudioBookName { get; set; } = null!;
        public string? UploadErrorText { get; set; }

        private async Task UploadFiles(InputFileChangeEventArgs e)
        {
            UploadErrorText = null;

            try
            {
                IsBusy = true;

                Files = e.GetMultipleFiles(int.MaxValue).Select(t => new UploadedFileInfo(t)).ToList();

                if (!Files.Any())
                {
                    return;
                }

                if (Files.Any(t => t.IsZip))
                {
                    if (Files.Count != 1)
                    {
                        UploadErrorText = "When uploading as zip than only single file is allowed.";
                        return;
                    }

                    await UploadAudioBookZip(Files.Single());
                }
                else
                {
                    await UploadAudioBookFiles(Files);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UploadAudioBookZip(UploadedFileInfo zip)
        {
            var file = zip.File;

            string? audioBookId;
            await using (var writeStream = Uploader.GetAudioBookZipWriteStream(AudioBookName, out audioBookId))
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

            _ = Uploader.UnzipAudioBook(audioBookId);
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

            _ = Downloader.GenerateZipForDownload(id, true);
        }

        private async Task ZipAll()
        {
            StartProgressBar("Creating archives for all audiobooks");
            var i = 0;
            try
            {
                IsBusy = true;
                await foreach (var audioBook in InfoProvider.GetAudioBooks())
                {
                    ProgressBarReport(++i);
                    await Downloader.GenerateZipForDownload(audioBook.Id);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }


        public string ProgressBarText { get; set; } = string.Empty;
        public int? ProgressBarTotal { get; set; }
        public int ProgressBarCurrent { get; set; }
        public bool IsBusy { get; set; }

        private async Task LoadFromFileSystem()
        {
            StartProgressBar("Loading files from file system into database.");
            var i = 0;
            try
            {
                IsBusy = true;
                await foreach (var audioBook in FileSystemInfoProvider.GetAudioBooks())
                {
                    ProgressBarReport(++i);
                    await InfoProvider.UpdateAudioBook(audioBook);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void StartProgressBar(string text)
        {
            ProgressBarText = text;
            ProgressBarCurrent = 0;
        }

        private void ProgressBarReport(int current, int? total = null)
        {
            ProgressBarCurrent = current;
            ProgressBarTotal = total;
        }

        private async Task ReplaceInfo()
        {
            try
            {
                IsBusy = true;

                await foreach (var audioBook in InfoProvider.GetAudioBooks())
                {
                    await using var enumerator = BookInfoProvider.GetBookInfoAsync(audioBook.Name).GetAsyncEnumerator();
                    // If MoveNextAsync() returns false, the enumerator is empty.
                    var first = await enumerator.MoveNextAsync() ? enumerator.Current : default;

                    if (first == null)
                    {
                        continue;
                    }

                    audioBook.Name = first.Title;
                    audioBook.AuthorName = first.Author;
                    audioBook.Description = first.Description;
                    audioBook.SeriesName = first.SeriesName;
                    audioBook.OrderInSeries = first.OrderInSeries;
                    audioBook.Rating = first.PercentageRating;
                    audioBook.Categories = first.Categories;

                    await InfoProvider.UpdateAudioBook(audioBook);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public record UploadedFileInfo(IBrowserFile File)
    {
        public bool IsZip => File.ContentType.ToLower().Contains("zip");
        public int UploadPercentage { get; set; }
    }
}
