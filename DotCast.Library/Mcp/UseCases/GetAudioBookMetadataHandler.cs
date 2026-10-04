using DotCast.Infrastructure.Persistence.Repositories;
using DotCast.Infrastructure.PersonalApiTokens.Authentication;
using DotCast.Library.Mcp.Models;
using DotCast.Library.Mcp.Specifications;
using DotCast.SharedKernel.Models;
namespace DotCast.Library.Mcp.UseCases;
public sealed class GetAudioBookMetadataHandler(IReadOnlyRepository<AudioBook> books, LibraryTokenAccess access)
{
    public async Task<AudioBookMetadata> Handle(GetAudioBookMetadata request, CancellationToken cancellationToken)
    {
        var user = await access.RequireAsync(PersonalTokenDefaults.ReadScope);
        if (string.IsNullOrWhiteSpace(request.Id) || request.Id.Length > 200) throw new ArgumentException("invalid_input");
        return await books.GetBySpecAsync(new AudioBookMetadataSpecification(request.Id, user.AvailableLibraries), cancellationToken)
            ?? throw new KeyNotFoundException("not_found");
    }
}
