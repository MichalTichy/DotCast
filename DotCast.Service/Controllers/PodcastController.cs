using System.Globalization;
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
                content.AppendLine("<li>");
                content.AppendLine($"{RemoveDiacritics(byAuthor.Key)}");
                content.AppendLine("<ul>");

                foreach (var podcastInfo in byAuthor.OrderBy(t => t.Name))
                {
                    content.AppendLine($"<li><a href=\"{podcastInfo.Url}\">{RemoveDiacritics(podcastInfo.Name)}</a></li>");
                }

                content.AppendLine("</ul>");
                content.AppendLine("</li>");
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

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(normalizedString.Length);

            for (var i = 0; i < normalizedString.Length; i++)
            {
                var c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}