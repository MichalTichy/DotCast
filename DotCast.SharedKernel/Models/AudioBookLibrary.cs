namespace DotCast.SharedKernel.Models;

public record AudioBookLibrary(
    IReadOnlyList<AudioBook> AudioBooks,
    IReadOnlyDictionary<string, AudioBookUserState> UserStates,
    FeaturedAudioBook? Featured);
