using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.BookInfoProvider.Base;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace DotCast.BookInfoProvider
{
    public class ApplyAudiobookSuggestionsToAllRequestHandler(
        IBookInfoProvider bookInfoProvider,
        IMessagePublisher messenger,
        ICurrentUserProvider<UserInfo> currentUserProvider,
        ILogger<ApplyAudiobookSuggestionsToAllRequestHandler> logger)
        : IMessageHandler<ApplyAudiobookSuggestionsToAllRequest, ApplyAudiobookSuggestionsToAllResult>
    {
        public async Task<ApplyAudiobookSuggestionsToAllResult> Handle(ApplyAudiobookSuggestionsToAllRequest message)
        {
            var user = await currentUserProvider.GetCurrentUserRequiredAsync();
            if (!user.IsAdmin)
            {
                throw new NotSupportedException("Only admins can do this action.");
            }

            var audioBooks = await messenger.RequestAsync<AudioBooksRetrievalRequest, IReadOnlyList<AudioBook>>(new AudioBooksRetrievalRequest());
            var strongMatches = 0;
            var updated = 0;
            var noSuggestions = 0;
            var weakMatches = 0;

            foreach (var audioBook in audioBooks)
            {
                var suggestions = await GetSuggestions(audioBook.AudioBookInfo.Name);
                if (suggestions.Count == 0)
                {
                    noSuggestions++;
                    continue;
                }

                var suggestion = suggestions.FirstOrDefault(candidate => IsStrongMatch(audioBook, candidate));
                if (suggestion == null)
                {
                    weakMatches++;
                    continue;
                }

                strongMatches++;
                if (!ApplySuggestion(audioBook, suggestion))
                {
                    continue;
                }

                logger.LogInformation(
                    "Applying strong metadata suggestion to audiobook {AudioBookId}: {Title} by {Author}",
                    audioBook.Id,
                    audioBook.AudioBookInfo.Name,
                    audioBook.AudioBookInfo.AuthorName);

                await messenger.ExecuteAsync(new AudioBookEdited(audioBook));
                updated++;
            }

            return new ApplyAudiobookSuggestionsToAllResult(audioBooks.Count, strongMatches, updated, noSuggestions, weakMatches);
        }

        private async Task<IReadOnlyList<FoundBookInfo>> GetSuggestions(string name)
        {
            var suggestions = new List<FoundBookInfo>();
            await foreach (var suggestion in bookInfoProvider.GetBookInfoAsync(name))
            {
                if (IsValidSuggestion(suggestion))
                {
                    suggestions.Add(suggestion);
                }
            }

            return suggestions;
        }

        private static bool IsStrongMatch(AudioBook audioBook, FoundBookInfo suggestion)
        {
            return string.Equals(
                       NormalizeForStrongMatch(audioBook.AudioBookInfo.Name),
                       NormalizeForStrongMatch(suggestion.Title),
                       StringComparison.Ordinal)
                   && string.Equals(
                       NormalizeForStrongMatch(audioBook.AudioBookInfo.AuthorName),
                       NormalizeForStrongMatch(suggestion.Author),
                       StringComparison.Ordinal);
        }

        private static bool IsValidSuggestion(FoundBookInfo suggestion)
        {
            return !string.IsNullOrWhiteSpace(suggestion.Title)
                   && !string.Equals(suggestion.Title, "ERROR", StringComparison.OrdinalIgnoreCase)
                   && !string.IsNullOrWhiteSpace(suggestion.Author)
                   && !string.Equals(suggestion.Author, "ERROR", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ApplySuggestion(AudioBook audioBook, FoundBookInfo suggestion)
        {
            var info = audioBook.AudioBookInfo;
            var changed = false;

            changed |= SetIfChanged(info.Name, suggestion.Title, value => info.Name = value);
            changed |= SetIfChanged(info.AuthorName, suggestion.Author, value => info.AuthorName = value);

            if (!string.IsNullOrWhiteSpace(suggestion.SeriesName))
            {
                changed |= SetIfChanged(info.SeriesName, suggestion.SeriesName, value => info.SeriesName = value);
            }

            if (!string.IsNullOrWhiteSpace(suggestion.Description))
            {
                changed |= SetIfChanged(info.Description, suggestion.Description, value => info.Description = value);
            }

            if (suggestion.OrderInSeries > 0)
            {
                changed |= SetIfChanged(info.OrderInSeries, suggestion.OrderInSeries, value => info.OrderInSeries = value);
            }

            if (suggestion.PercentageRating > 0)
            {
                changed |= SetIfChanged(audioBook.Rating, suggestion.PercentageRating, value => audioBook.Rating = value);
            }

            if (suggestion.Categories.Count > 0 && !HaveSameCategories(info.Categories, suggestion.Categories))
            {
                info.Categories = suggestion.Categories.ToList();
                changed = true;
            }

            return changed;
        }

        private static bool HaveSameCategories(ICollection<Category> currentCategories, ICollection<Category> suggestionCategories)
        {
            return currentCategories
                .Select(category => category.Name)
                .Order(StringComparer.Ordinal)
                .SequenceEqual(
                    suggestionCategories.Select(category => category.Name).Order(StringComparer.Ordinal),
                    StringComparer.Ordinal);
        }

        private static bool SetIfChanged<T>(T currentValue, T newValue, Action<T> setValue)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
            {
                return false;
            }

            setValue(newValue);
            return true;
        }

        private static string NormalizeForStrongMatch(string value)
        {
            var normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);
            var previousWasWhitespace = false;

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsWhiteSpace(character))
                {
                    if (!previousWasWhitespace)
                    {
                        builder.Append(' ');
                        previousWasWhitespace = true;
                    }

                    continue;
                }

                builder.Append(char.ToUpperInvariant(character));
                previousWasWhitespace = false;
            }

            return builder.ToString().Trim().Normalize(NormalizationForm.FormC);
        }
    }
}
