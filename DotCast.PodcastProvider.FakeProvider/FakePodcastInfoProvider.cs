using Bogus;
using DotCast.PodcastProvider.Base;

namespace DotCast.PodcastProvider.FakeProvider
{
    public class FakePodcastInfoProvider : IPodcastInfoProvider
    {
        public async IAsyncEnumerable<PodcastInfo> GetPodcasts(string? searchText = null)
        {
            var faker = GetFaker();
            for (var i = 0; i < 50; i++)
            {
                yield return faker.Generate();
            }

            await Task.CompletedTask; //async enumerable requires awaited statement ...
        }

        private static Faker<PodcastInfo> GetFaker()
        {
            return new Faker<PodcastInfo>().CustomInstantiator(faker =>
                new PodcastInfo(faker.Random.Hash(), faker.Company.CompanyName(0), faker.Person.FullName, faker.Internet.Url(), faker.Image.PlaceholderUrl(500, 500),
                    TimeSpan.FromMinutes(faker.Random.Double(20, 400))));
        }

        public Task UpdatePodcastInfo(PodcastInfo podcastInfo)
        {
            return Task.CompletedTask;
        }

        public Task<PodcastInfo> Get(string id)
        {
            var faker = GetFaker();
            faker.RuleFor(info => info.Id, () => id);
            var result = faker.Generate();
            return Task.FromResult(result);
        }
    }
}