using System.Text;
using System.Web.Http;
using DotCast.Service.PodcastProviders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotCast.Service.Controllers
{
    public class PodcastController : ApiController
    {
        private IPodcastProvider podcastProvider;
        private readonly ILogger logger;

        public PodcastController(IPodcastProvider podcastProvider, ILogger<PodcastController> logger)
        {
            this.podcastProvider = podcastProvider;
            this.logger = logger;
        }

        [Microsoft.AspNetCore.Mvc.Route("podcast/{podcastName}")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public ContentResult Get(string podcastName)
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
                ContentType = "application/xml"
            };

        }
        
        [Microsoft.AspNetCore.Mvc.Route("podcasts")]
        public ContentResult Get()
        {
            var podcastsNames = podcastProvider.GetPodcastNames();

            var content = new StringBuilder();


            content.AppendLine("<html>");
            content.AppendLine("<body>");
            content.AppendLine("<ul>");
            foreach (var podcastsName in podcastsNames)
            {
                content.AppendLine($"<li><a href=\"./podcast/{podcastsName}\">{podcastsName.Replace('_', ' ')}</a></li>");
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