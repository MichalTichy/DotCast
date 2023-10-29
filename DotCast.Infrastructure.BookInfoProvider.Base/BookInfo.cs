using DotCast.AudioBookInfo;

namespace DotCast.Infrastructure.BookInfoProvider.Base
{
    public record BookInfo(string Title, string Author, string? Description, string? SeriesName, int OrderInSeries, string? ImgUrl, int PercentageRating, List<Category> Categories)
    {
    }
}
