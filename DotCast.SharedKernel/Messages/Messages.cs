using DotCast.SharedKernel.Models;

namespace DotCast.SharedKernel.Messages
{
    //WEB
    public record AudioBooksRetrievalRequest(string? Filter = null);

    public record AudioBookDetailRequest(string Id);

    public record AudioBookEdited(AudioBook AudioBook);

    public record NewAudioBookIdRequest(string Name);

    public record AudioBookUploadStartRequest(string AudioBookId, ICollection<string> Files);

    public record AudioBooksStatisticsRequest;

    public record RestoreFromFileSystemRequest;

    public record ReprocessAllAudioBooksRequest(bool Unzip = false);

    public record AudiobookInfoSuggestionsRequest(string Name, int? Count = null);


    //STORAGE
    public record AudioBookStorageMetadataUpdated(AudioBookInfo AudioBookInfo);

    public record FileUploaded(string AudioBookId, string FileName);

    public record AudioBookReadyForProcessing(string AudioBookId, ICollection<string> ModifiedFiles);

    public record ProcessingStatusChanged(ICollection<string> RunningProcessings);

    //LIBRARY
    public record AudioBookRssRequest(string Id);
}