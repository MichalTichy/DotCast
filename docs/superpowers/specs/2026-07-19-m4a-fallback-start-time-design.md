# M4A fallback start-time fix

## Problem

When `Mp4Chapters` cannot extract chapter metadata from an M4A file, `Mp4aSplitter` creates a fallback chapter whose timestamp is the source audio duration. The existing conversion calculation treats that timestamp as the chapter start and uses the same source duration as the chapter end, producing a zero-length FFmpeg request. FFmpeg can report success while creating an approximately 0.05-second MP3, after which the processing step deletes the valid M4A source.

The supplied audiobook demonstrates this behavior for its chapterless M4A tracks. The source ZIP and audio streams are valid.

## Scope

This change is limited to correcting the fallback chapter start time.

In scope:

- Set the fallback chapter timestamp to `TimeSpan.Zero`
- Build the application for Release `linux-x64`
- Validate the corrected behavior against one track from the supplied audiobook ZIP
- Build and push `registry.tichymichal.net/dotcast:latest`

Out of scope:

- Repairing or re-uploading the already-processed audiobook
- Changing source-file deletion behavior
- Changing playlist generation
- Adding an automated MP4 processing test project
- Replacing or upgrading `mp4chap`
- Refactoring the conversion pipeline or its interfaces

## Approaches considered

### 1. Correct only the fallback timestamp

This is the approved approach. Replace the source duration with `TimeSpan.Zero` when constructing the fallback chapter. It directly fixes the faulty time range with the smallest possible production change.

### 2. Add transactional conversion and output validation

This would validate generated media before deleting source files and could make conversion failures recoverable. It was not selected because it broadens the requested change beyond the confirmed defect.

### 3. Preserve chapterless M4A files

This would avoid transcoding when chapter metadata is unavailable. It was not selected because it changes DotCast's current M4A-to-MP3 processing behavior.

## Design

Update the fallback branch in `DotCast.Storage.Processing.Steps.MP4A/Mp4aSplitter.cs` so the single synthetic chapter starts at `TimeSpan.Zero`.

The existing calculations then produce:

- `startTime = TimeSpan.Zero`
- `endTime = source audio duration`
- `duration = source audio duration`

The existing FFmpeg invocation will therefore encode the complete source audio into one MP3. No public interfaces, dependency registrations, package references, or pipeline ordering change.

## Error handling

Existing error behavior remains unchanged. Chapter extraction failures continue to be logged as warnings and use the single-file fallback. FFmpeg conversion failures continue to propagate through the current processing pipeline.

## Validation

Validation will include:

1. Start the Aspire application before making the production code change to verify the baseline environment, as required by the repository workflow.
2. Build `DotCast.App` for Release `linux-x64`.
3. Extract one M4A track from the supplied ZIP into a temporary directory.
4. Exercise the corrected fallback conversion and inspect the result with FFprobe.
5. Confirm that the output duration represents the full source track and is not the previously reproduced approximately 0.05-second output.
6. Remove temporary validation artifacts.
7. Build the Docker image from the published application output.
8. Push `registry.tichymichal.net/dotcast:latest` and confirm the registry accepts the manifest.
