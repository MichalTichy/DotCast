using System.Runtime.CompilerServices;
using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.SharedKernel.Models;
namespace DotCast.Library.Mcp.Tests;
public sealed class StubBookInfoProvider(string source, Func<CancellationToken, Task>? before = null, int count = 2) : IBookInfoProvider
{
    public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public async IAsyncEnumerable<FoundBookInfo> GetBookInfoAsync(string name, string? author = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default, int maxResults = 10)
    {
        Started.TrySetResult();
        if (before is not null) await before(cancellationToken);
        for (var i = 0; i < Math.Min(count, maxResults); i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new FoundBookInfo($"{source} {i}", "Author", null, null, 0, null, 50, [], source);
        }
    }
}
