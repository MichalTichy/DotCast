using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Castle.Core.Internal;
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
        public string AuthorEmail
        {
            get => Generator.AuthorEmail;
            set => Generator.AuthorEmail = value;
        }
        public string HomepageUrl
        {
            get => Generator.HomepageUrl;
            set => Generator.HomepageUrl = value;
        }
        public string ImageUrl
        {
            get => Generator.ImageUrl;
            set => Generator.ImageUrl = value;
        }
        public string iTunesCategory
        {
            get => Generator.iTunesCategory;
            set => Generator.iTunesCategory = value;
        }
        public string iTunesSubCategory
        {
            get => Generator.iTunesSubCategory;
            set => Generator.iTunesSubCategory = value;
        }
        public string Title
        {
            get => Generator.Title;
            set => Generator.Title = value;
        }
        public string Description
        {
            get => Generator.Description;
            set => Generator.Description = value;
        }

        protected RssGenerator Generator;

        public Feed()
        {
            Generator = new RssGenerator()
            {
                Episodes = new List<Episode>(),
                Categories = new List<string>(),
            };
        }

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