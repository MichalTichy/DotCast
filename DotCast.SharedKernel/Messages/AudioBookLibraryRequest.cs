using DotCast.SharedKernel.Models;

namespace DotCast.SharedKernel.Messages;

public record AudioBookLibraryRequest(AudioBookLibraryFilter? Filter = null);
