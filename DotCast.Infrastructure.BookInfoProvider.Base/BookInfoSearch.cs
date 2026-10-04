using System.Runtime.CompilerServices;
using DotCast.SharedKernel.Models;
namespace DotCast.Infrastructure.BookInfoProvider.Base;
public static class BookInfoSearch
{
    public static bool IsValid(FoundBookInfo info) =>
        !string.IsNullOrWhiteSpace(info.Title) && !string.Equals(info.Title, "ERROR", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(info.Author) && !string.Equals(info.Author, "ERROR", StringComparison.OrdinalIgnoreCase);

    public static async IAsyncEnumerable<FoundBookInfo> LoadDetailsAsync<T>(
        IAsyncEnumerable<T> search, Func<T, CancellationToken, Task<FoundBookInfo>> load,
        int maxResults, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (maxResults is < 1 or > 10) throw new ArgumentOutOfRangeException(nameof(maxResults));
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await using var candidates = search.GetAsyncEnumerator(stop.Token);
        var pending = new Queue<Task<FoundBookInfo>>();
        var fetched = 0;
        var returned = 0;
        var exhausted = false;
        try
        {
            while (true)
            {
                while (!exhausted && fetched < 10 && pending.Count < Math.Min(3, maxResults))
                {
                    if (!await candidates.MoveNextAsync()) { exhausted = true; break; }
                    pending.Enqueue(load(candidates.Current, stop.Token));
                    fetched++;
                }
                if (pending.Count == 0) yield break;
                // Await in search order, even when later pages finish first.
                var info = await pending.Dequeue();
                if (!IsValid(info)) continue;
                yield return info;
                if (++returned >= maxResults) yield break;
            }
        }
        finally
        {
            stop.Cancel();
            // Early disposal owns these requests: cancel and observe them before returning.
            try { await Task.WhenAll(pending); }
            catch (Exception) when (stop.IsCancellationRequested) { }
        }
    }
}
