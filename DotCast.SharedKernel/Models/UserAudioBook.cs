using DotCast.Infrastructure.Persistence;

namespace DotCast.SharedKernel.Models
{
    public class UserAudioBook : IItemWithId
    {
        public const int MinRating = 1;
        public const int MaxRating = 10;

        public required string Id { get; init; }
        public required string AudioBookId { get; init; }
        public required string UserId { get; init; }

        public int? Rating { get; set; }
        public DateTime? RatedAt { get; set; }
        public DateTime? NotInterestedAt { get; set; }

        public static string BuildId(string audioBookId, string userId) => $"{audioBookId}:{userId}";

        public static UserAudioBook Create(string audioBookId, string userId)
        {
            return new UserAudioBook
            {
                Id = BuildId(audioBookId, userId),
                AudioBookId = audioBookId,
                UserId = userId
            };
        }

        public static bool IsValidRating(int rating) => rating is >= MinRating and <= MaxRating;

        public void Rate(int rating, DateTime timestampUtc)
        {
            if (!IsValidRating(rating))
            {
                throw new ArgumentOutOfRangeException(nameof(rating), rating, $"Rating must be between {MinRating} and {MaxRating}.");
            }

            Rating = rating;
            RatedAt = timestampUtc;
        }

        public void ClearRating()
        {
            Rating = null;
            RatedAt = null;
        }

        public void MarkNotInterested(DateTime timestampUtc)
        {
            NotInterestedAt = timestampUtc;
        }

        public void ClearNotInterested()
        {
            NotInterestedAt = null;
        }
    }
}
