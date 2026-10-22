namespace DotCast.SharedKernel.Models
{
    public record BookToRateItem(
        string AudioBookId,
        string Name,
        string? Description,
        string? ImageUrl,
        DateTime LastActivityAt);
}
