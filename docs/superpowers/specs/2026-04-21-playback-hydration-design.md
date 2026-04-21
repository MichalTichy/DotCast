# Playback hydration fix for Admin Active Playbacks

## Problem

Admin Active Playbacks does not reflect persisted playback state correctly after a full archive download. The archive download event fires, `ArchiveRead` is processed successfully, and the persisted document reaches:

- `Status = CloseToFinished`
- `HasDownloadedFinalFile = true`
- `LastFileDownloadedAt` set to the archive read timestamp

But the Admin UI still shows values consistent with a default-hydrated document, such as `InfoRetrieved`, no last file timestamp, and `Final file = No`.

The likely root cause is that `AudioBookPlayback` stores persisted mutable state behind private setters, while Marten hydration on document load is not restoring those values in this code path.

## Scope

This change is limited to the playback-state persistence bug.

In scope:

- Ensure `AudioBookPlayback` round-trips persisted state correctly through Marten
- Preserve the current event-driven update flow
- Keep the Admin Active Playbacks query simple and based on loaded document state

Out of scope:

- Aspire local URL/base URL mismatches
- Query-side workarounds for bad hydration
- New tests for this change

## Recommended approach

Relax setter visibility for the persisted mutable properties on `AudioBookPlayback` so Marten can hydrate the stored values reliably when the document is reloaded.

Targeted properties:

- `Status`
- `FirstRssGeneratedAt`
- `LastRssGeneratedAt`
- `LastFileDownloadedAt`
- `LastArchiveReadAt`
- `HasDownloadedFinalFile`
- `FinishedAt`

This keeps the fix close to the persistence boundary and addresses the root cause instead of compensating in the query layer.

## Alternatives considered

### 1. Relax setter visibility on persisted state

This is the recommended option. It is the smallest change with the most direct relationship to the observed failure. The tradeoff is weaker encapsulation on the model, but the existing domain methods still define the intended state transitions.

### 2. Keep private setters and customize Marten mapping/serialization

This would preserve stricter encapsulation, but it increases framework-specific coupling and requires confidence that the chosen Marten configuration is the established pattern in this repository.

### 3. Work around the issue in the active-playback query

This is not recommended. It would treat the symptom in `ActivePlaybacksRequestHandler` rather than fixing incorrect model hydration and would make the query path harder to reason about.

## Design

### Model changes

Update `DotCast.SharedKernel.Models.AudioBookPlayback` so its persisted mutable properties use setters that Marten can populate during deserialization.

The domain methods remain the intended write path:

- `RegisterRssGenerated`
- `RegisterFileDownloaded`
- `RegisterArchiveRead`
- `MarkFinished`

No new mutation helpers are required for this fix.

### Query behavior

No changes are required in `ActivePlaybacksRequestHandler`.

Once the document hydrates correctly, the existing logic should naturally show:

- `CloseToFinished` after a full archive download
- a populated last file timestamp sourced from `LastFileDownloadedAt`
- `HasDownloadedFinalFile = true`

### Storage behavior

No Marten schema redesign is required. Existing duplicated fields such as `Status` remain in place. The change is limited to allowing the document payload to hydrate the full persisted state consistently.

## Error handling

No new error handling is introduced. Existing event handlers continue to ignore invalid messages in the same places they do today. The fix changes how valid persisted state is materialized after load.

## Validation

Validation for this change should focus on the existing playback flow:

1. A full archive download publishes and handles `ArchiveRead`
2. The stored playback document contains `CloseToFinished`, `HasDownloadedFinalFile = true`, and a last file timestamp
3. Admin Active Playbacks reflects those stored values after reloading the document

Per the approved scope, this design does not add automated tests.
