namespace DotCast.SharedKernel.Models
{
    public record AudioBookUserState(
        string AudioBookId,
        ListeningState ListeningState,
        int? Rating,
        bool NotInterested,
        DisplayedRating DisplayedRating);
}
