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

    public record AudiobookInfoSuggestionsRequest(string Name, int? Count = null);


    //STORAGE
    public record AudioBookStorageMetadataUpdated(AudioBook AudioBook);

    public record FileUploaded(string AudioBookId, string FileName);

    public record FilesModificationsFinished(string AudioBookId, ICollection<string> ModifiedFiles);

    //LIBRARY
    public record AudioBookRssRequest(string Id);
}