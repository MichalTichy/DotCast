using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;
using DotCast.Storage.Abstractions;

namespace DotCast.Library.Handlers;

public class MaintenancePreviewRequestHandler(IStorage storage, ICurrentUserProvider<UserInfo> currentUserProvider,
    INoTenancyReadOnlyRepository<AudioBook> books) : IMessageHandler<MaintenancePreviewRequest, IReadOnlyList<AudioBookBasicInfo>>
{
    public async Task<IReadOnlyList<AudioBookBasicInfo>> Handle(MaintenancePreviewRequest message)
    {
        var user = await currentUserProvider.GetCurrentUserRequiredAsync();
        if (!user.IsAdmin) throw new UnauthorizedAccessException();
        var result = new List<AudioBookBasicInfo>();
        foreach (var entry in storage.GetEntries())
        {
            var book = await books.GetByIdAsync(entry.Id);
            result.Add(new(entry.Id, book?.AudioBookInfo.Name));
        }
        return result.OrderBy(book => book.Name).ToList();
    }
}
