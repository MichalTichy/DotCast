namespace DotCast.Library.Playback
{
    public interface IAudioBookUserContextProvider
    {
        Task<AudioBookUserContext> GetForCurrentUserAsync(CancellationToken cancellationToken = default);
    }
}
