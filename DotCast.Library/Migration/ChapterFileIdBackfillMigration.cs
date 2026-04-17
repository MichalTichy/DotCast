using DotCast.Infrastructure.Persistence.Marten.Extensions;
using DotCast.Infrastructure.Persistence.Marten.Migration;
using Marten;
using Microsoft.Extensions.Logging;

namespace DotCast.Library.Migration;

public class ChapterFileIdBackfillMigration(ILogger<ChapterFileIdBackfillMigration> logger) : IMartenMigration
{
    public int Version => 2026041501;

    public async Task MigrateAsync(IDocumentSession session, CancellationToken ct)
    {
        await using var command = session.GetConnection().CreateCommand();
        command.CommandText = """
            update mt_doc_audio_book
            set data = jsonb_set(
                data,
                '{AudioBookInfo,Chapters}',
                (
                    select coalesce(
                        jsonb_agg(
                            case
                                when chapter ? 'FileId' then chapter - 'Url'
                                when chapter ? 'Url' then
                                    (chapter - 'Url') || jsonb_build_object(
                                        'FileId',
                                        regexp_replace(
                                            split_part(
                                                split_part(chapter->>'Url', '?', 1),
                                                '#',
                                                1
                                            ),
                                            '^.*/',
                                            ''
                                        )
                                    )
                                else chapter
                            end
                        ),
                        '[]'::jsonb
                    )
                    from jsonb_array_elements(coalesce(data #> '{AudioBookInfo,Chapters}', '[]'::jsonb)) as chapter
                ),
                false
            )
            where exists (
                select 1
                from jsonb_array_elements(coalesce(data #> '{AudioBookInfo,Chapters}', '[]'::jsonb)) as chapter
                where chapter ? 'Url'
            );
            """;

        var updated = await command.ExecuteNonQueryAsync(ct);
        logger.LogInformation("Backfilled chapter file identifiers for {Count} audiobook documents.", updated);
    }
}
