using DotCast.SharedKernel.Models;

namespace DotCast.Library.Playback
{
    /// <summary>
    /// Playback signals and personal data (rating, interest) of a single user, keyed by audiobook ID.
    /// </summary>
    public record AudioBookUserContext(
        IReadOnlyDictionary<string, AudioBookPlayback> Playbacks,
        IReadOnlyDictionary<string, UserAudioBook> UserAudioBooks)
    {
        public static AudioBookUserContext Empty { get; } = new(
            new Dictionary<string, AudioBookPlayback>(),
            new Dictionary<string, UserAudioBook>());

        public static AudioBookUserContext Create(IEnumerable<AudioBookPlayback> playbacks, IEnumerable<UserAudioBook> userAudioBooks)
        {
            return new AudioBookUserContext(
                playbacks.ToDictionary(x => x.AudioBookId),
                userAudioBooks.ToDictionary(x => x.AudioBookId));
        }

        public AudioBookPlayback? GetPlayback(string audioBookId) => Playbacks.GetValueOrDefault(audioBookId);

        public int? GetRating(string audioBookId) => UserAudioBooks.GetValueOrDefault(audioBookId)?.Rating;

        public bool IsNotInterested(string audioBookId) => UserAudioBooks.GetValueOrDefault(audioBookId)?.NotInterestedAt != null;

        public ListeningState GetListeningState(string audioBookId) => ListeningStateResolver.Resolve(GetPlayback(audioBookId));

        public DisplayedRating GetDisplayedRating(AudioBook audioBook) => DisplayedRating.From(audioBook, GetRating(audioBook.Id));

        public AudioBookUserState GetState(AudioBook audioBook)
        {
            return new AudioBookUserState(
                audioBook.Id,
                GetListeningState(audioBook.Id),
                GetRating(audioBook.Id),
                IsNotInterested(audioBook.Id),
                GetDisplayedRating(audioBook));
        }
    }
}
