using System.Runtime.CompilerServices;
using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.SharedKernel.Models;
namespace DotCast.Library.Mcp.Tests;
public sealed class TestBookInfoProvider : IBookInfoProvider
{
    public CancellationToken LastCancellationToken { get; private set; }
    public int Produced { get; private set; }
    public async IAsyncEnumerable<FoundBookInfo> GetBookInfoAsync(string name, string? author = null, [EnumeratorCancellation] CancellationToken cancellationToken = default, int maxResults = 10)
    {
        LastCancellationToken = cancellationToken;
        Produced = 0;
        for (var index = 0; index < maxResults; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            Produced++;
            yield return new FoundBookInfo($"Suggested {index}", "Author", null, null, 0, null, 50, []);
        }
    }
}
