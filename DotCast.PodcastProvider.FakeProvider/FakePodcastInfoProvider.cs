using Bogus;
using DotCast.PodcastProvider.Base;

namespace DotCast.PodcastProvider.FakeProvider
{
    public class FakePodcastInfoProvider : IPodcastInfoProvider
    {
        public IEnumerable<PodcastInfo> GetPodcasts()
        {
            var podcastInfos = new List<PodcastInfo>();
            var faker = new Faker<PodcastInfo>().CustomInstantiator(faker =>
                new PodcastInfo(faker.Company.CompanyName(0), faker.Person.FullName, faker.Internet.Url(), faker.Image.PlaceholderUrl(500, 500),
                    TimeSpan.FromMinutes(faker.Random.Double(20, 400))));
            for (var i = 0; i < 50; i++)
            {
                podcastInfos.Add(faker.Generate());
            }

            return podcastInfos;
        }
    }
}