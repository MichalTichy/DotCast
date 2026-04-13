namespace DotCast.SharedKernel.Models
{
    public record AudioBookLibraryFacets(
        IReadOnlyCollection<string> Authors,
        IReadOnlyCollection<string> Categories,
        IReadOnlyCollection<string> Series);
}
