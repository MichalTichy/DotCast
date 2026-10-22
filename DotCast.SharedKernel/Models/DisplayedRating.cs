namespace DotCast.SharedKernel.Models
{
    /// <summary>
    /// Rating shown to the current user: their own 1-10 rating when present, otherwise the general percentage rating.
    /// </summary>
    public record DisplayedRating(int Value, bool IsPersonal)
    {
        public static DisplayedRating From(AudioBook audioBook, int? personalRating)
        {
            return personalRating.HasValue
                ? new DisplayedRating(personalRating.Value, true)
                : new DisplayedRating(audioBook.Rating, false);
        }

        public int ToPercent() => IsPersonal ? Value * 10 : Value;

        public override string ToString() => IsPersonal ? $"★ {Value}/10" : $"{Value} %";
    }
}
