using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class AudioBooksStatisticsRequestHandler(IReadOnlyRepository<AudioBook> repository) : IMessageHandler<AudioBooksStatisticsRequest, AudioBooksStatistics>
    {
        public async Task<AudioBooksStatistics> Handle(AudioBooksStatisticsRequest message)
        {
            var specification = new AudioBookStatisticSpecification();
            return await repository.GetBySpecAsync(specification) ?? new AudioBooksStatistics(0, 0, TimeSpan.Zero);
        }
    }
}