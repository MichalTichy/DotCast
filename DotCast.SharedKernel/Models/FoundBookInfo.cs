
namespace DotCast.SharedKernel.Models
{
    public record FoundBookInfo(string Title, string Author, string? Description, string? SeriesName, int OrderInSeries, string? ImgUrl, int PercentageRating, ICollection<Category> Categories);
}