namespace DotCast.SharedKernel.Models
{
    public record FeaturedAudioBook(
        AudioBook AudioBook,
        FeaturedAudioBookReason Reason,
        string? ContinuesAudioBookName = null);
}
