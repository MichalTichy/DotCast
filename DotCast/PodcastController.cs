using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace DotCast
{
    public class PodcastController : ApiController
    {
        private IPodcastProvider podcastProvider;
        private string baseUrl;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            baseUrl = controllerContext.Request.RequestUri.GetLeftPart(UriPartial.Authority);
            this.podcastProvider = new LocalPodcastProvider(AppConfiguration.PodcastsLocation, baseUrl);

        }


        [Route("podcast/{podcastName}")]
        public HttpResponseMessage Get(string podcastName)
        {
            var feed = podcastProvider.GetFeed(podcastName);

            if (feed == null)
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ReasonPhrase = "Requested podcast not found!"
                };
            }

            return new HttpResponseMessage()
            {
                Content = new StringContent(feed.Generate(), Encoding.UTF8, "application/xml")
            };
        }
        [Route("podcasts")]
        public HttpResponseMessage Get()
        {
            var podcastsNames = podcastProvider.GetPodcastNames();

            var content = new StringBuilder();


            content.AppendLine("<html>");
            content.AppendLine("<body>");
            content.AppendLine("<ul>");
            foreach (var podcastsName in podcastsNames)
            {
                content.AppendLine($"<li><a href=\"{baseUrl}/{ControllerContext.ControllerDescriptor.ControllerName}/{podcastsName}\">{podcastsName}</a></li>");
            }

            content.AppendLine("</ul>");
            content.AppendLine("</body>");
            content.AppendLine("</html>");
            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content.ToString(), Encoding.UTF8, "text/html")
            };

        }
    }
}