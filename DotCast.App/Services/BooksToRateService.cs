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
        public IReadOnlyList<BookToRateItem> Items { get; private set; } = [];
        public int Count => Items.Count;
        public bool IsOpen { get; private set; }

        /// <summary>Raised when the list or the dialog visibility changes.</summary>
        public event Action? Changed;

        /// <summary>Raised when the user rated or otherwise changed a book from outside the library page.</summary>
        public event Func<Task>? AudioBookChanged;

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            Items = await messenger.RequestAsync<BooksToRateRequest, IReadOnlyList<BookToRateItem>>(new BooksToRateRequest(), cancellationToken);
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
            await RefreshAfterChangeAsync();
        }

        public async Task ConfirmNotFinishedAsync(string audioBookId)
        {
            await messenger.ExecuteAsync(new AudioBookNotFinishedRequest(audioBookId));
            await RefreshAfterChangeAsync();
        }

        public async Task RefreshAfterChangeAsync(CancellationToken cancellationToken = default)
        {
            await RefreshAsync(cancellationToken);
            if (AudioBookChanged is { } handlers)
            {
                foreach (var handler in handlers.GetInvocationList().Cast<Func<Task>>())
                {
                    await handler();
                }
            }
        }
    }
}
