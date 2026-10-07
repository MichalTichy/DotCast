using DotCast.Infrastructure.Persistence;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;

namespace DotCast.Library.Handlers
{
    internal static class AudioBookPlaybackRepositoryExtensions
    {
        public static async Task StoreAsync<T>(this INoTenancyRepository<T> repository, T item) where T : IItemWithId
        {
            var existing = await repository.GetByIdAsync(item.Id);
            if (existing == null)
            {
                await repository.AddAsync(item);
                return;
            }

            await repository.UpdateAsync(item);
        }
    }
}
