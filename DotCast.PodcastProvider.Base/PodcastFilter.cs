namespace DotCast.PodcastProvider.Base
{
    public static class PodcastFilter
    {
        public static bool Matches(PodcastInfo input, string searchedText)
        {
            var normalizedSearchText = searchedText.ToLower();
            return input.Name.ToLower().Contains(normalizedSearchText) ||
                   input.AuthorName.ToLower().Contains(normalizedSearchText);
        }
    }
}