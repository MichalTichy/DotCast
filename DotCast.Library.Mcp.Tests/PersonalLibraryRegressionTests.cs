using System.Reflection;
using DotCast.App.Pages;
using DotCast.Infrastructure.Persistence.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DotCast.Library.Mcp.Tests;

public class PersonalLibraryRegressionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UserCommand_WaitsForPlaybackPersistence(bool rate)
    {
        var context = new PersonalLibraryTestContext();
        var saving = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        context.BeforePlaybackSave = () => { saving.SetResult(); return release.Task; };

        var command = rate
            ? context.Messenger.Object.ExecuteAsync(new RateAudioBookRequest(context.Book.Id, 3))
            : context.Messenger.Object.ExecuteAsync(new SetAudioBookListenedRequest(context.Book.Id, true));
        try
        {
            await saving.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(command.IsCompleted);
        }
        finally { release.TrySetResult(); }
        await command;

        var library = await context.Library.Handle(new());
        Assert.Equal(ListeningState.Finished, library.UserStates[context.Book.Id].ListeningState);
        if (rate) Assert.Equal(3, library.UserStates[context.Book.Id].Rating);
    }

    [Fact]
    public async Task PlaybackPersistenceFailure_ReachesTheCaller()
    {
        var context = new PersonalLibraryTestContext { BeforePlaybackSave = () => throw new IOException("Playback write failed") };
        await Assert.ThrowsAsync<IOException>(() => context.BooksToRate.RateAsync(context.Book.Id, 3));
    }

    [Fact]
    public async Task RatingFromDetail_RefreshesPersistedListeningStateAndActiveRatingFilter()
    {
        var context = new PersonalLibraryTestContext();
        using var renderer = new TestRenderer();
        var page = CreatePage(context, renderer);
        Set(page, "MinRating", 80);
        await renderer.Dispatcher.InvokeAsync(() => Invoke(page, "RateSelectedAsync", (int?)3));

        Assert.Null(Get<string?>(page, "UserActionMessage"));
        Assert.Null(Get<string?>(page, "LoadMessage"));
        Assert.Empty(page.Data);
        var state = Get<IReadOnlyDictionary<string, AudioBookUserState>>(page, "UserStates")[context.Book.Id];
        Assert.Equal(3, state.Rating);
        Assert.Equal(ListeningState.Finished, state.ListeningState);

        await renderer.Dispatcher.InvokeAsync(() => Invoke(page, "RateSelectedAsync", (int?)null));
        Assert.Single(page.Data);
        Assert.Null(Get<IReadOnlyDictionary<string, AudioBookUserState>>(page, "UserStates")[context.Book.Id].Rating);
        Assert.Equal(1, context.BooksToRate.Count);
    }

    [Fact]
    public async Task DialogRating_RefreshesTheLibraryBeforeReturning()
    {
        var context = new PersonalLibraryTestContext();
        using var renderer = new TestRenderer();
        var page = CreatePage(context, renderer);
        Set(page, "MinRating", 80);

        await context.BooksToRate.RateAsync(context.Book.Id, 3);

        Assert.Empty(page.Data);
        Assert.Null(Get<string?>(page, "LoadMessage"));
        Assert.Equal(0, context.BooksToRate.Count);
    }

    [Fact]
    public async Task ListenedChange_ReappliesListeningFilter()
    {
        var context = new PersonalLibraryTestContext();
        using var renderer = new TestRenderer();
        var page = CreatePage(context, renderer);
        Set(page, "ListeningState", ListeningStateFilter.Unplayed);

        await renderer.Dispatcher.InvokeAsync(() => Invoke(page, "ToggleListenedAsync"));

        Assert.Empty(page.Data);
        Assert.Null(Get<string?>(page, "UserActionMessage"));
        Assert.Null(Get<string?>(page, "LoadMessage"));
        Assert.Equal(1, context.BooksToRate.Count);
        await renderer.Dispatcher.InvokeAsync(() => Invoke(page, "ToggleListenedAsync"));
        Assert.Equal(PlaybackStatus.InProgress, context.Playback!.Status);
        Assert.Equal(0, context.BooksToRate.Count);
    }

    [Fact]
    public async Task NotFinishedConfirmation_AllowsCandidateToReturnAfterNewPlaybackSignal()
    {
        var context = new PersonalLibraryTestContext();
        await context.Messenger.Object.ExecuteAsync(new SetAudioBookListenedRequest(context.Book.Id, true));
        await context.BooksToRate.RefreshAsync();
        Assert.Equal(1, context.BooksToRate.Count);

        await context.BooksToRate.ConfirmNotFinishedAsync(context.Book.Id);
        Assert.Equal(0, context.BooksToRate.Count);
        var signalAt = context.Playback!.NotFinishedConfirmedAt!.Value.AddSeconds(1);
        context.Playback.RegisterArchiveRead(signalAt);
        context.Playback.MarkFinished(signalAt);
        await context.BooksToRate.RefreshAsync();

        Assert.Equal(1, context.BooksToRate.Count);
    }

    [Fact]
    public async Task LibraryQuery_LoadsBooksAndPersonalContextOnce()
    {
        var context = new PersonalLibraryTestContext();
        var library = await context.Library.Handle(new());

        Assert.Single(library.AudioBooks);
        Assert.Single(library.UserStates);
        Assert.NotNull(library.Featured);
        context.Books.Verify(x => x.ListAsync(It.IsAny<IListSpecification<AudioBook>>(), default, null), Times.Once);
        context.UserContext.Verify(x => x.GetForCurrentUserAsync(default), Times.Once);
    }

    private static AudioBooks CreatePage(PersonalLibraryTestContext context, TestRenderer renderer)
    {
        var page = new EmptyPage
        {
            Messenger = context.Messenger.Object, BooksToRate = context.BooksToRate,
            Js = null!, LibraryApiInfoProvider = null!, CurrentUserProvider = null!, NavigationManager = null!,
            Data = [context.Book], SelectedAudioBook = context.Book
        };
        var listener = typeof(AudioBooks).GetMethod("OnAudioBookChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
        context.BooksToRate.AudioBookChanged += (Func<Task>)listener.CreateDelegate(typeof(Func<Task>), page);
        renderer.Attach(page);
        return page;
    }

    private static T Get<T>(AudioBooks page, string property) =>
        (T)typeof(AudioBooks).GetProperty(property, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(page)!;
    private static void Set(AudioBooks page, string property, object value) =>
        typeof(AudioBooks).GetProperty(property, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(page, value);
    private static Task Invoke(AudioBooks page, string method, params object?[] args) =>
        (Task)typeof(AudioBooks).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(page, args)!;

    private sealed class EmptyPage : AudioBooks
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder) { }
        protected override Task OnAfterRenderAsync(bool firstRender) => Task.CompletedTask;
    }

#pragma warning disable BL0006 // Minimal renderer attaches the real page event handlers without a browser.
    private sealed class TestRenderer() : Renderer(new ServiceCollection().BuildServiceProvider(), NullLoggerFactory.Instance)
    {
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
        public int Attach(IComponent component) => AssignRootComponentId(component);
        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
        protected override void HandleException(Exception exception) => throw exception;
    }
#pragma warning restore BL0006
}
