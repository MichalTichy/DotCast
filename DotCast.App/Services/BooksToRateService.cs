using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.App.Services
{
    /// <summary>
    /// Circuit-scoped state of books the current user is asked to rate. Shared by the layout, the library page and the rating dialog.
    /// </summary>
    public class BooksToRateService(IMessagePublisher messenger)
    {
        // Books answered in this circuit; hides them even before queued playback events are processed.
        private readonly HashSet<string> handledAudioBookIds = new();

        public IReadOnlyList<BookToRateItem> Items { get; private set; } = [];
        public int Count => Items.Count;
        public bool IsOpen { get; private set; }

        /// <summary>Raised when the list or the dialog visibility changes.</summary>
        public event Action? Changed;

        /// <summary>Raised when the user rated or otherwise changed a book from outside the library page.</summary>
        public event Action? AudioBookChanged;

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            var items = await messenger.RequestAsync<BooksToRateRequest, IReadOnlyList<BookToRateItem>>(new BooksToRateRequest(), cancellationToken);
            Items = items.Where(item => !handledAudioBookIds.Contains(item.AudioBookId)).ToList();
            Changed?.Invoke();
        }

        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            await RefreshAsync(cancellationToken);
            if (Items.Count == 0)
            {
                return;
            }

            IsOpen = true;
            Changed?.Invoke();
        }

        public void Close()
        {
            IsOpen = false;
            Changed?.Invoke();
        }

        public async Task RateAsync(string audioBookId, int rating)
        {
            await messenger.ExecuteAsync(new RateAudioBookRequest(audioBookId, rating));
            MarkHandled(audioBookId);
        }

        public async Task ConfirmNotFinishedAsync(string audioBookId)
        {
            await messenger.ExecuteAsync(new AudioBookNotFinishedRequest(audioBookId));
            MarkHandled(audioBookId);
        }

        /// <summary>Call after a book was rated elsewhere (e.g. the book detail) so the pending count stays accurate.</summary>
        public void MarkHandled(string audioBookId)
        {
            handledAudioBookIds.Add(audioBookId);
            Items = Items.Where(item => item.AudioBookId != audioBookId).ToList();
            Changed?.Invoke();
            AudioBookChanged?.Invoke();
        }

        /// <summary>Call when a book may have become a rating candidate again (rating cleared, marked as listened).</summary>
        public async Task ReconsiderAsync(string audioBookId, CancellationToken cancellationToken = default)
        {
            handledAudioBookIds.Remove(audioBookId);

            // Playback changes are processed on a background queue; give it a moment before re-reading candidates.
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            await RefreshAsync(cancellationToken);
        }
    }
}
