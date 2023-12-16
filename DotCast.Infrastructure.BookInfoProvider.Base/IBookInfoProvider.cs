using DotCast.SharedKernel.Models;

namespace DotCast.Infrastructure.BookInfoProvider.Base
{
    public interface IBookInfoProvider
    {
        IAsyncEnumerable<FoundBookInfo> GetBookInfoAsync(string name);
    }
}
