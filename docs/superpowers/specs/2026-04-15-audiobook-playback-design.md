# Audiobook Playback Tracking Design

## Goal

Add a best-effort audiobook playback tracking model that infers progress from RSS generation and file downloads without claiming true playback accuracy.

This tracking is not user-facing yet. The first consumer is an admin-only `Active Playbacks` tab.

## Current Context

The current system has:

- Shared audiobook metadata in `AudioBook` and `AudioBookInfo`
- Per-user signed RSS URLs generated in `LibraryApiInformationProvider`
- Signed chapter file URLs generated in `StorageApiInformationProvider`
- Anonymous file download handling in `DownloadFileEndpoint`

There is no persisted per-user playback state today. Audiobook metadata is shared across users, so playback evidence must not be stored on `AudioBook`.

## Recommended Approach

Use a dedicated per-user playback evidence model and update it from factual edge events:

1. RSS link generated
2. Audiobook file downloaded
3. Manual finish action

This approach keeps shared audiobook metadata clean, isolates user-specific state, and gives the strongest practical best-effort signal available without controlling the podcast player.

## Rejected Alternatives

### Infer directly from existing audiobook metadata

Rejected because playback is per user and would pollute shared audiobook state.

### Store only timestamps without a playback model

Rejected because timestamps alone do not provide a durable query surface for admin review or future filtering by playback state.

### Track full chapter-by-chapter playback history

Rejected for the first version because it adds more storage and logic than needed. The approved first version only needs enough evidence to support `InfoRetrieved`, `InProgress`, `CloseToFinished`, and manual `Finished`.

## Domain Model

Add a new persisted model: `AudioBookPlayback`.

Suggested fields:

- `Id`
- `AudioBookId`
- `UserId`
- `Status`
- `FirstRssGeneratedAt`
- `LastRssGeneratedAt`
- `LastFileDownloadedAt`
- `HasDownloadedFinalFile`
- `FinishedAt`

### Identity

`AudioBookPlayback` is logically unique per `AudioBookId + UserId`.

Implementation can use either:

- a composite uniqueness rule, if the persistence layer makes that convenient, or
- a deterministic single `Id` derived from `AudioBookId` and `UserId`

The important requirement is one playback record per audiobook per user.

## Status Model

Add an enum with these values:

- `InfoRetrieved`
- `InProgress`
- `CloseToFinished`
- `Finished`

### Meaning

- `InfoRetrieved`: RSS/feed information was generated for the user
- `InProgress`: at least one audiobook file was downloaded by the user
- `CloseToFinished`: the final audiobook file was downloaded by the user
- `Finished`: manually confirmed by a user action

### Transition Rules

Automatic transitions:

- first RSS generation -> `InfoRetrieved`
- first audiobook file download -> `InProgress`
- final audiobook file download -> `CloseToFinished`

Manual transition:

- explicit finish action -> `Finished`

Rules:

- automatic handlers must never overwrite `Finished`
- `HasDownloadedFinalFile = true` is raw evidence and should remain stored even after manual finish
- `FinishedAt` is set only by the manual finish action

## Events

Add these messages:

- `AudioBookRssLinkGenerated(AudioBookId, UserId, Timestamp)`
- `AudioBookFileDownloaded(AudioBookId, UserId, FileId, Timestamp)`
- `AudioBookMarkedFinished(AudioBookId, UserId, Timestamp)`

These events should remain factual. The final-file heuristic belongs in the playback handler, not in the event payload.

## Event Flow

### 1. RSS generation

Source: `LibraryApiInformationProvider`

When a signed RSS URL is generated, publish `AudioBookRssLinkGenerated`.

Handler behavior:

- upsert `AudioBookPlayback`
- set `FirstRssGeneratedAt` if missing
- update `LastRssGeneratedAt`
- set status to `InfoRetrieved` only if there is no stronger automatic state already

### 2. File download

Source: `DownloadFileEndpoint`

When an audiobook file is downloaded, publish `AudioBookFileDownloaded`.

Handler behavior:

- upsert `AudioBookPlayback`
- update `LastFileDownloadedAt`
- load the audiobook metadata
- determine whether the downloaded file is the final chapter file
- if final file, set `HasDownloadedFinalFile = true` and `Status = CloseToFinished`
- otherwise set `Status = InProgress`

### 3. Manual finish

Source: future explicit action

When a user manually marks the audiobook finished, publish `AudioBookMarkedFinished`.

Handler behavior:

- set `Status = Finished`
- set `FinishedAt`

## File Download Attribution Constraint

This is the most important implementation detail in the design.

`DownloadFileEndpoint` is currently anonymous, and signed file URLs currently identify only the resource, not the user. That means publishing `AudioBookFileDownloaded` from the endpoint is not sufficient unless the request can be attributed to a user.

The design therefore requires user attribution to be carried through the signed file URL flow.

Recommended change:

- extend signed download URL generation so the download request carries a user identifier as part of the signed URL contract
- validate that identifier together with the signature at download time
- emit `AudioBookFileDownloaded` with the attributed `UserId`

This should be implemented at the download boundary, not guessed later from unrelated context.

## Matching Downloaded File To Final File

The playback handler should determine whether the downloaded file is the final audiobook file by comparing the downloaded file identifier to the final chapter entry in `AudioBookInfo.Chapters`.

This comparison should be based on a stable file identifier already present in the download route or file URL, not on display names.

## Query Surface

Playback data is not shown to normal users in this version.

Add query support so admin pages can retrieve active playback rows joined with audiobook and user information.

Suggested admin projection fields:

- audiobook name
- user name
- playback status
- `LastRssGeneratedAt`
- `LastFileDownloadedAt`
- `HasDownloadedFinalFile`
- `FinishedAt`

## Admin UI

Add an admin-only `Active Playbacks` tab on the admin page.

First version requirements:

- read-only
- shows one row per `AudioBookPlayback`
- lists records where `Status != Finished`
- sorted by `LastFileDownloadedAt` descending, then `LastRssGeneratedAt` descending

This is intentionally operational rather than user-facing.

## Error Handling

Playback handlers should be idempotent:

- repeated RSS generation events should only update timestamps
- repeated file download events should not create duplicate playback records
- repeated final-file downloads should not change a stronger status back to a weaker one

If the file download handler cannot resolve the audiobook or final-file match:

- retain factual download timing if possible
- avoid incorrectly promoting to `CloseToFinished`
- log the mismatch for later inspection

If a playback record is already `Finished`, automatic handlers should update non-conflicting timestamps only if desired, but must not downgrade status.

## Testing

Add focused tests for:

- RSS event creates playback with `InfoRetrieved`
- repeated RSS event updates `LastRssGeneratedAt` without resetting `FirstRssGeneratedAt`
- non-final file download promotes playback to `InProgress`
- final file download promotes playback to `CloseToFinished` and sets `HasDownloadedFinalFile`
- manual finish promotes playback to `Finished` and sets `FinishedAt`
- automatic events do not downgrade `Finished`
- file download attribution fails safely when user context is missing or invalid

## Scope Boundaries

Included in scope:

- playback model
- status enum
- events and handlers
- user attribution in signed file download flow
- admin `Active Playbacks` tab

Not included in scope:

- user-facing playback UI
- full chapter-by-chapter playback history
- true playback position tracking
- review integration
- player-side callbacks

## Recommendation Summary

Implement playback as a separate per-user evidence model driven by RSS generation and attributed file downloads. Treat final-file download as `CloseToFinished`, and reserve `Finished` for a manual user transition. Expose the resulting rows only on an admin `Active Playbacks` tab in the first release.
