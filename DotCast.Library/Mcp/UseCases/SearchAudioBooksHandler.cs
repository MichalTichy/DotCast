using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Library.Mcp.Models;
using DotCast.Library.Mcp.Specifications;
using DotCast.SharedKernel.Models;
namespace DotCast.Library.Mcp.UseCases;
public sealed class SearchAudioBooksHandler(IReadOnlyRepository<AudioBook> books, ICurrentUserProvider<UserInfo> users)
{
    public async Task<AudioBookSearchPage> Handle(SearchAudioBooks request, CancellationToken cancellationToken)
    {
        var user = await users.GetCurrentUserRequiredAsync();
        if (request.Offset < 0 || request.Limit is < 1 or > 100 || request.Search?.Length > 200 ||
            request.Category?.Length > 200 || request.LibraryId?.Length > 200) throw new ArgumentException("invalid_input");
        return await books.GetBySpecAsync(new AudioBooksSearchSpecification(request, user.AvailableLibraries), cancellationToken)
            ?? new AudioBookSearchPage([], 0, request.Offset, request.Limit);
    }
}
