using System.Text;
using System.Xml;

namespace DotCast.RssGenerator.Base
{
    public abstract class RssGenerator<TParam>
    {
        public abstract Task<Feed> BuildFeed(TParam param);

        public async Task<string> GenerateRss(TParam param)
        {
            var feed = await BuildFeed(param);

            var sb = new StringBuilder();
            await using (var xmlWriter = XmlWriter.Create(sb))
            {
                Generate(feed, xmlWriter);
            }

            return sb.ToString();
        }

        private void Generate(Feed feed, XmlWriter writer)
        {
            var itunesUri = "http://www.itunes.com/dtds/AudioBook-1.0.dtd";

            // Start document
            writer.WriteStartDocument();

            // Start rss
            writer.WriteStartElement("rss");
            writer.WriteAttributeString("xmlns", "itunes", null, itunesUri);
            writer.WriteAttributeString("version", "2.0");

            // Start channel
            writer.WriteStartElement("channel");

            writer.WriteElementString("title", feed.Title);
            writer.WriteElementString("description", feed.Description);
            writer.WriteElementString("link", feed.HomepageUrl);
            writer.WriteElementString("language", feed.Language);
            writer.WriteElementString("copyright", feed.Copyright);
            writer.WriteElementString("lastBuildDate", DateTime.UtcNow.ToString("r"));
            writer.WriteElementString("pubDate", DateTime.UtcNow.ToString("r"));
            writer.WriteElementString("webMaster", feed.AuthorEmail);

            // Start image
            writer.WriteStartElement("image");

            writer.WriteElementString("url", feed.ImageUrl);
            writer.WriteElementString("title", feed.Title);
            writer.WriteElementString("link", feed.HomepageUrl);

            writer.WriteElementString("width", feed.ImageWidth.ToString());
            writer.WriteElementString("height", feed.ImageHeight.ToString());
            writer.WriteElementString("description", feed.Description);


            // End image
            writer.WriteEndElement();

            // Categories

            foreach (var category in feed.Categories ?? Array.Empty<string>())
            {
                writer.WriteElementString("Category", category);
            }

            writer.WriteElementString("author", itunesUri, feed.AuthorName);
            writer.WriteElementString("subtitle", itunesUri, feed.SubTitle);
            writer.WriteElementString("summary", itunesUri, feed.Description);

            // Start itunes:owner
            writer.WriteStartElement("owner", itunesUri);

            writer.WriteElementString("name", itunesUri, feed.AuthorName);
            writer.WriteElementString("email", itunesUri, feed.AuthorEmail);

            // End  itunes:owner
            writer.WriteEndElement();

            writer.WriteElementString("explicit", itunesUri, feed.IsExplicit ? "Yes" : "No");

            // Start itunes:image
            writer.WriteStartElement("image", itunesUri);

            writer.WriteAttributeString("href", feed.ImageUrl);

            // End itunes:image
            writer.WriteEndElement();

            // iTunes category
            // Start itunes:category
            writer.WriteStartElement("category", itunesUri);
            writer.WriteAttributeString("text", feed.TunesCategory);

            // Start itunes:category
            writer.WriteStartElement("category", itunesUri);
            writer.WriteAttributeString("text", feed.TunesSubCategory);
            // End itunes:category
            writer.WriteEndElement();

            // End itunes:category
            writer.WriteEndElement();

            foreach (var episode in feed.Episodes ?? Array.Empty<Episode>())
            {
                // Start AudioBook item
                writer.WriteStartElement("item");

                writer.WriteElementString("title", episode.Title);
                writer.WriteElementString("link", episode.Permalink);
                writer.WriteElementString("guid", episode.Permalink);
                writer.WriteElementString("description", episode.Summary);

                // Start enclosure 
                writer.WriteStartElement("enclosure");

                writer.WriteAttributeString("url", episode.FileUrl);
                writer.WriteAttributeString("length", episode.FileLength.ToString());
                writer.WriteAttributeString("type", episode.FileType);

                // End enclosure
                writer.WriteEndElement();

                writer.WriteElementString("pubDate", episode.PublicationDate.ToString("r"));

                writer.WriteElementString("author", itunesUri, feed.AuthorName);
                writer.WriteElementString("explicit", itunesUri, episode.IsExplicit ? "Yes" : "No");
                writer.WriteElementString("subtitle", itunesUri, episode.SubTitle);
                writer.WriteElementString("summary", itunesUri, episode.Summary);
                writer.WriteElementString("duration", itunesUri, episode.Duration.ToString("g"));
                if (!string.IsNullOrEmpty(episode.Keywords))
                    writer.WriteElementString("keywords", itunesUri, episode.Keywords);

                // End AudioBook item
                writer.WriteEndElement();
            }

            // End channel
            writer.WriteEndElement();

            // End rss
            writer.WriteEndElement();

            // End document
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }
    }
}
