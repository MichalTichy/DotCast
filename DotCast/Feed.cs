using System.Collections.Generic;
using System.Text;
using System.Xml;
using PodcastRssGenerator4DotNet;

namespace DotCast
{
    public class Feed
    {
        public List<Episode> Episodes
        {
            get => Generator.Episodes;
            set => Generator.Episodes = value;
        }

        public string AuthorName
        {
            get => Generator.AuthorName;
            set => Generator.AuthorName = value;
        }
        public string Title
        {
            get => Generator.Title;
            set => Generator.Title = value;
        }
        protected RssGenerator Generator = new RssGenerator();

        public string Generate()
        {
            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb))
            {
                Generator.Generate(xmlWriter);
            }

            return sb.ToString();
        }
    }
}