using DotCast.App.Services;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;
using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Infrastructure.Persistence.Specifications;
using DotCast.Library.Handlers;
using DotCast.Library.Playback;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.Extensions.Options;
using Moq;

namespace DotCast.Library.Mcp.Tests;

internal sealed class PersonalLibraryTestContext
{
    public const string UserId = "user";
    public AudioBook Book { get; } = MetadataPatchTests.Book();
    public AudioBookPlayback? Playback { get; private set; }
    public UserAudioBook? PersonalBook { get; private set; }
    public Func<Task> BeforePlaybackSave { get; set; } = () => Task.CompletedTask;
    public Mock<IMessagePublisher> Messenger { get; } = new();
    public Mock<IReadOnlyRepository<AudioBook>> Books { get; } = new();
    public Mock<IAudioBookUserContextProvider> UserContext { get; } = new();
    public BooksToRateService BooksToRate { get; }
    public AudioBookLibraryRequestHandler Library { get; }

    public PersonalLibraryTestContext()
    {
        Book.Rating = 90;
        var currentUser = new Mock<ICurrentUserIdProvider>();
        currentUser.Setup(x => x.GetCurrentUserIdRequiredAsync()).ReturnsAsync(UserId);
        Books.Setup(x => x.GetRequiredByIdAsync(Book.Id, default, null, null)).ReturnsAsync(Book);
        Books.Setup(x => x.ListAsync(It.IsAny<IListSpecification<AudioBook>>(), default, null)).ReturnsAsync([Book]);

        var personalBooks = new Mock<INoTenancyRepository<UserAudioBook>>();
        personalBooks.Setup(x => x.GetByIdAsync(It.IsAny<string>(), default, null)).ReturnsAsync(() => PersonalBook);
        personalBooks.Setup(x => x.UpsertAsync(It.IsAny<UserAudioBook>(), default, null))
            .Callback((UserAudioBook book, CancellationToken _, string? _) => PersonalBook = book);

        var playbacks = new Mock<INoTenancyRepository<AudioBookPlayback>>();
        var playbackReads = playbacks.As<INoTenancyReadOnlyRepository<AudioBookPlayback>>();
        playbacks.Setup(x => x.GetByIdAsync(It.IsAny<string>(), default, null)).ReturnsAsync(() => Playback);
        playbackReads.Setup(x => x.GetByIdAsync(It.IsAny<string>(), default, null)).ReturnsAsync(() => Playback);
        playbacks.Setup(x => x.UpsertAsync(It.IsAny<AudioBookPlayback>(), default, null))
            .Returns(async (AudioBookPlayback playback, CancellationToken _, string? _) =>
            {
                await BeforePlaybackSave();
                Playback = playback;
            });
        playbacks.Setup(x => x.UpdateAsync(It.IsAny<AudioBookPlayback>(), default, null))
            .Returns(async (AudioBookPlayback playback, CancellationToken _, string? _) =>
            {
                await BeforePlaybackSave();
                Playback = playback;
            });

        var finished = new AudioBookPlaybackMarkedFinishedHandler(playbacks.Object);
        var unfinished = new AudioBookPlaybackMarkedUnfinishedHandler(playbacks.Object);
        var notFinished = new AudioBookPlaybackNotFinishedConfirmedHandler(playbacks.Object);
        Messenger.Setup(x => x.ExecuteAsync(It.IsAny<AudioBookPlaybackMarkedFinished>(), It.IsAny<CancellationToken>()))
            .Returns((AudioBookPlaybackMarkedFinished message, CancellationToken _) => finished.Handle(message));
        Messenger.Setup(x => x.ExecuteAsync(It.IsAny<AudioBookPlaybackMarkedUnfinished>(), It.IsAny<CancellationToken>()))
            .Returns((AudioBookPlaybackMarkedUnfinished message, CancellationToken _) => unfinished.Handle(message));
        Messenger.Setup(x => x.ExecuteAsync(It.IsAny<AudioBookPlaybackNotFinishedConfirmed>(), It.IsAny<CancellationToken>()))
            .Returns((AudioBookPlaybackNotFinishedConfirmed message, CancellationToken _) => notFinished.Handle(message));

        var rate = new RateAudioBookRequestHandler(currentUser.Object, Books.Object, personalBooks.Object, playbackReads.Object, Messenger.Object);
        var clear = new ClearAudioBookRatingRequestHandler(currentUser.Object, personalBooks.Object);
        var listened = new SetAudioBookListenedRequestHandler(currentUser.Object, Books.Object, Messenger.Object);
        var confirm = new AudioBookNotFinishedRequestHandler(currentUser.Object, Books.Object, Messenger.Object);
        Messenger.Setup(x => x.ExecuteAsync(It.IsAny<RateAudioBookRequest>(), It.IsAny<CancellationToken>()))
            .Returns((RateAudioBookRequest message, CancellationToken _) => rate.Handle(message));
        Messenger.Setup(x => x.ExecuteAsync(It.IsAny<ClearAudioBookRatingRequest>(), It.IsAny<CancellationToken>()))
            .Returns((ClearAudioBookRatingRequest message, CancellationToken _) => clear.Handle(message));
        Messenger.Setup(x => x.ExecuteAsync(It.IsAny<SetAudioBookListenedRequest>(), It.IsAny<CancellationToken>()))
            .Returns((SetAudioBookListenedRequest message, CancellationToken _) => listened.Handle(message));
        Messenger.Setup(x => x.ExecuteAsync(It.IsAny<AudioBookNotFinishedRequest>(), It.IsAny<CancellationToken>()))
            .Returns((AudioBookNotFinishedRequest message, CancellationToken _) => confirm.Handle(message));

        UserContext.Setup(x => x.GetForCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => AudioBookUserContext.Create(
                Playback is { } playback ? [playback] : [], PersonalBook is { } personal ? [personal] : []));
        var options = Options.Create(new FinishedListeningOptions());
        Library = new(Books.Object, UserContext.Object, options);
        var toRate = new BooksToRateRequestHandler(Books.Object, UserContext.Object, options);
        Messenger.Setup(x => x.RequestAsync<AudioBookLibraryRequest, AudioBookLibrary>(It.IsAny<AudioBookLibraryRequest>(), It.IsAny<CancellationToken>()))
            .Returns((AudioBookLibraryRequest message, CancellationToken _) => Library.Handle(message));
        Messenger.Setup(x => x.RequestAsync<BooksToRateRequest, IReadOnlyList<BookToRateItem>>(It.IsAny<BooksToRateRequest>(), It.IsAny<CancellationToken>()))
            .Returns((BooksToRateRequest message, CancellationToken _) => toRate.Handle(message));
        BooksToRate = new(Messenger.Object);
    }
}
