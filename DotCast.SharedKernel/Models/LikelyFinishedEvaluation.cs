namespace DotCast.SharedKernel.Models
{
    public record LikelyFinishedEvaluation(bool IsLikelyFinished, string? Reason)
    {
        public static LikelyFinishedEvaluation No { get; } = new(false, null);
    }
}
