namespace DotCast.SharedKernel.Models
{
    public static class ListeningStateResolver
    {
        public static ListeningState Resolve(AudioBookPlayback? playback)
        {
            return playback?.Status switch
            {
                null or PlaybackStatus.InfoRetrieved => ListeningState.Unplayed,
                PlaybackStatus.Finished => ListeningState.Finished,
                _ => ListeningState.InProgress
            };
        }

        public static bool Matches(ListeningState state, ListeningStateFilter filter)
        {
            return filter switch
            {
                ListeningStateFilter.Unplayed => state == ListeningState.Unplayed,
                ListeningStateFilter.InProgress => state == ListeningState.InProgress,
                ListeningStateFilter.Finished => state == ListeningState.Finished,
                _ => true
            };
        }
    }
}
