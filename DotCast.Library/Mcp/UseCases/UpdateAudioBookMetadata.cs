using DotCast.Library.Mcp.Models;
namespace DotCast.Library.Mcp.UseCases;
public sealed record UpdateAudioBookMetadata(string Id, AudioBookMetadataPatch Patch);
