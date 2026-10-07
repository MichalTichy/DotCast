namespace DotCast.SharedKernel.Messages;

public record ProcessingJobChanged(string AudioBookId, bool? Succeeded, DateTime TimestampUtc);
