using System.Linq;
using System.Text;
using System.Web.Http;
using DotCast.Service.PodcastProviders;
using DotCast.Service.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotCast.Service.Controllers
{
    public class PodcastController : ApiController
    {
        private IPodcastProvider podcastProvider;
        private readonly ILogger logger;
        private readonly PodcastProviderSettings settings;

        public PodcastController(IPodcastProvider podcastProvider, ILogger<PodcastController> logger, PodcastProviderSettings settings)
        {
            this.podcastProvider = podcastProvider;
            this.logger = logger;
            this.settings = settings;
        }

        [Route("podcast/{podcastName}")]
        [Authorize]
        public ActionResult<string> Get(string podcastName)
        {
            
            logger.LogInformation($"Serving podcast: {podcastName}");
            
            var feed = podcastProvider.GetFeed(podcastName);

            if (feed == null)
            {
                return new ContentResult()
                {
                    StatusCode = 404,
                    Content = "Requested podcast not found!"
                };
            }

            return new ContentResult()
            {
                StatusCode = 200,
                Content = feed.Generate(),
                ContentType = "application/xml; charset=utf-16"
            };

        }
        
        [Route("podcasts")]
        public ContentResult Get()
        {
            var podcastsNames = podcastProvider.GetPodcastInfo();

            var content = new StringBuilder();


            content.AppendLine("<html>");
            content.AppendLine("<body>");
            content.AppendLine("<ul>");
            foreach (var byAuthor in podcastsNames.GroupBy(t => t.AuthorName))
            {
                content.AppendLine($"<li>{byAuthor.Key}</li>");
                content.AppendLine("<ul>");

                foreach (var podcastInfo in byAuthor.OrderBy(t => t.Name))
                {
                    content.AppendLine($"<li><a href=\"{podcastInfo.Url}</a></li>");
                }

                content.AppendLine("</ul>");
            }

            content.AppendLine("</ul>");
            content.AppendLine("</body>");
            content.AppendLine("</html>");
            return new ContentResult()
            {
                StatusCode = 200,
                Content = content.ToString(),
                ContentType = "text/html"
            };


        }
    }
}