using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.Extensions.Logging;
namespace DotCast.BookInfoProvider;
public class AudiobookInfoSuggestionsRequestHandler(
    IEnumerable<IBookInfoProvider> bookInfoProviders, ILogger<AudiobookInfoSuggestionsRequestHandler> logger)
{
    public async Task<IReadOnlyCollection<FoundBookInfo>> Handle(AudiobookInfoSuggestionsRequest message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (message.Count <= 0) return [];
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var tasks = bookInfoProviders.Select(provider => ReadProviderAsync(provider, message, stop.Token)).ToArray();
        var result = new List<FoundBookInfo>();
        var failures = new List<Exception>();
        try
        {
            // Providers run together, but the existing source preference stays deterministic.
            foreach (var task in tasks)
            {
                var outcome = await task;
                if (outcome.Failure is not null) failures.Add(outcome.Failure);
                foreach (var info in outcome.Items)
                {
                    result.Add(info);
                    if (result.Count >= message.Count) return result;
                }
            }
            if (result.Count == 0 && tasks.Length > 0 && failures.Count == tasks.Length)
                throw new HttpRequestException("Book information providers are unavailable.", failures[0]);
            return result;
        }
        finally
        {
            stop.Cancel();
            try { await Task.WhenAll(tasks); }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        }
    }
    private async Task<ProviderOutcome> ReadProviderAsync(IBookInfoProvider provider, AudiobookInfoSuggestionsRequest request, CancellationToken cancellationToken)
    {
        var items = new List<FoundBookInfo>();
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(TimeSpan.FromSeconds(10));
        try
        {
            var limit = Math.Min(request.Count ?? 10, 10);
            await foreach (var info in provider.GetBookInfoAsync(request.Name, request.AuthorName, budget.Token, limit))
            {
                if (!BookInfoSearch.IsValid(info)) continue;
                items.Add(info);
                if (items.Count >= limit) break;
            }
            return new(items, null);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Book information provider {Provider} failed", provider.GetType().Name);
            return new(items, exception);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Book information provider {Provider} timed out", provider.GetType().Name);
            return new(items, exception);
        }
    }
    private sealed record ProviderOutcome(IReadOnlyList<FoundBookInfo> Items, Exception? Failure);
}
