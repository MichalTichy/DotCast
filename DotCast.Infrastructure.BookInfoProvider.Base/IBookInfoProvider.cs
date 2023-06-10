namespace DotCast.Infrastructure.BookInfoProvider.Base
{
    public interface IBookInfoProvider
    {
        IAsyncEnumerable<BookInfo> GetBookInfoAsync(string name);
    }
}