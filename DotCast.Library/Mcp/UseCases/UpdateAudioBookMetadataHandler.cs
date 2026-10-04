using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.PersonalApiTokens.Authentication;
using DotCast.Library.Mcp.Models;
using DotCast.Library.Mcp.Persistence;
using Microsoft.Extensions.Logging;
namespace DotCast.Library.Mcp.UseCases;
public sealed class UpdateAudioBookMetadataHandler(ITransactionalAudioBookMetadataWriter writer, LibraryTokenAccess access,
    IPersonalTokenContext tokens, IMessagePublisher messenger, ILogger<UpdateAudioBookMetadataHandler> logger)
{
    public async Task<AudioBookMetadata> Handle(UpdateAudioBookMetadata request, CancellationToken cancellationToken)
    {
        var user = await access.RequireAsync(PersonalTokenDefaults.WriteScope);
        if (string.IsNullOrWhiteSpace(request.Id) || request.Id.Length > 200) throw new ArgumentException("invalid_input");
        request.Patch.Validate();
        var metadata = await writer.UpdateAsync(request.Id, user.UsersLibraryName, request.Patch, cancellationToken);
        var fields = request.Patch.Changes.Keys.Order().ToArray();
        logger.LogInformation("MCP token {TokenId} owner {OwnerId} updated audiobook {BookId} fields {Fields}",
            tokens.RequireScope(PersonalTokenDefaults.WriteScope).TokenId, user.Id, request.Id, string.Join(",", fields));
        await messenger.PublishAsync(new AudioBookMetadataUpdated(request.Id, user.Id, fields));
        return metadata;
    }
}
