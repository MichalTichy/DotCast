using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using Shared.Infrastructure.CurrentUserProvider;
using Shared.Infrastructure.Persistence.Repositories;
using Wolverine;

namespace DotCast.Library.Handlers
{
    public class AudioBookStorageMetadataUpdatedHandler(IRepository<AudioBook> repository, ICurrentUserProvider<UserInfo> userProvider) : IMessageHandler<AudioBookStorageMetadataUpdated>
    {
        public async Task Handle(AudioBookStorageMetadataUpdated message)
        {
            var audioBook = await repository.GetByIdAsync(message.AudioBookInfo.Id);
            if (audioBook == null)
            {
                var user = await userProvider.GetCurrentUserRequiredAsync();
                await repository.AddAsync(new AudioBook
                {
                    Id = message.AudioBookInfo.Id,
                    AudioBookInfo = message.AudioBookInfo,
                    LibraryId = user.UsersLibraryName
                });
            }
            else
            {
                audioBook.AudioBookInfo = message.AudioBookInfo;
                await repository.UpdateAsync(audioBook);
            }
        }
    }
}