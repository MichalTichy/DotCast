using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace DotCast
{
    public class PodcastController : ApiController
    {
        private IEpisodeProvider episodeProvider;

        public PodcastController()
        {
            this.episodeProvider = new LocalEpisodeProvider(AppConfiguration.BasePath);
        }

        public HttpResponseMessage Get(string podcastName)
        {
            var feed = episodeProvider.GetFeed(podcastName);
            return new HttpResponseMessage()
            {
                Content = new StringContent(feed.Generate(), Encoding.UTF8, "application/xml")
            };
        }
    }

    public static class AppConfiguration
    {
        public const string BasePath = "/files";
    }
}