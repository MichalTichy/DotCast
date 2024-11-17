using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;

namespace DotCast.Library.Handlers
{
    public class AudioBooksStatisticsRequestHandler
        (IReadOnlyRepository<AudioBook> repository, ILogger<AudioBooksStatisticsRequestHandler> logger) : IMessageHandler<AudioBooksStatisticsRequest, AudioBooksStatistics>
    {
        public async Task<AudioBooksStatistics> Handle(AudioBooksStatisticsRequest message)
        {
            try
            {
                var specification = new AudioBookStatisticSpecification();
                return (await repository.GetBySpecAsync(specification))!;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to retrieve statistics.");
                return new AudioBooksStatistics(0, 0, TimeSpan.Zero);
            }
        }
    }
}