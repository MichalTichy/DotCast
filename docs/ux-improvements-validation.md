# UX improvements — implementation and validation

Implemented the 18 recommendations in `AiTesting/2026-10-04/ux-review/ux-suggestions.md`. The instruction to hide suggestions supersedes recommendations 3 and 4: suggestion retrieval, comparison, application and bulk admin controls are absent throughout the web UI. Backend provider/MCP operations remain available. Changes are local, not a deployment.

## Environment

Validated on 5 October 2026 against local Aspire at `https://localhost:7062/`, using development seed accounts and disposable fixtures. Login was verified by protected library content and authenticated admin navigation. Visited Library, New Audiobook, Account, Admin and audiobook editors; opened all filter sections, details, Metadata/Files/Chapters, Maintenance/Active playbacks, draft-exit and maintenance/playback confirmation dialogs. The live signed-in instance was not modified.

Requirements came from the live review, pages/components, message handlers and storage endpoints. No ADO requirements source is configured. This is targeted UX validation, not an exhaustive certification of unrelated APIs, credentials or all devices.

## Coverage ledger

| Flow | Evidence | Verdict |
|---|---|---|
| Build | Final app build: 0 errors; 84 existing dependency/compatibility warnings | PASS |
| Suggestions | No suggestion UI references in Pages/Shared; editor/admin snapshots have no suggestion controls | PASS |
| Mobile library | At 390×844 search spans content width, filters start collapsed, first book choice appears in first viewport, totals follow results | PASS |
| Search | Martan matches accented Marťan and excludes Artemis description mention; integration tests verify author matches and foreign-library exclusion | PASS |
| Rating filters | Unrated and Below 60% produce distinct results; aria-pressed and removable chips verified | PASS |
| Category filters | Detective Stories, Crime yields one title, survives reload as one category through repeated category query key; chip removal restores six titles | PASS |
| Other filters | Author/category search and duration controls inspected; query/selected-state code reviewed. All slider positions and filter combinations unexercised | INCONCLUSIVE |
| Empty results | Nonmatching search gives 0 titles and Clear filters; clear restores fixture titles | PASS |
| Browsing | Grid, compact list, author rails, full-author grid and author jump exercised; long title wraps completely; title/cover open details; series badge visible | PASS |
| Sort variants | Title sort exercised; author/series, rating, duration and recent source reviewed; other variants not independently asserted in browser | INCONCLUSIVE |
| Return context | Save and exit preserves sort=title and view=list. Query stores filters/expanded authors; JS saves URL-scoped scroll, rails and disclosures | PASS |
| Scroll restoration variants | Implemented, but every rail offset/return path/viewport combination was not measured | INCONCLUSIVE |
| Details | Compact mobile dialog, podcast instructions, personal feed URL, copy control, Download ZIP label and expandable synopsis inspected | PASS |
| Clipboard/archive | Copy invoked; clipboard contents and an actual ZIP transfer not independently verified | INCONCLUSIVE |
| Dialog keyboard | Initial Close focus; Shift+Tab contained after correction; Escape closes; originating button regains focus; native modal background inert | PASS |
| Editor draft | Dirty indicator; Back opens Keep editing/Save and leave/Discard and leave; Keep editing retains draft; successful save clears dirty state | PASS |
| External navigation | NavigationLock binds unload confirmation to dirty state; browser-owned external warning not exercised | INCONCLUSIVE |
| Save failure | Failed Save and exit stays in editor and retains draft; regression ensures storage failure cannot update database or queue processing; inaccessible save rejected | PASS |
| Numeric validation | Required name and rating/series limits implemented in UI/handler; every invalid numeric entry not exercised | INCONCLUSIVE |
| Chapters | Natural sorting marks draft; real MP3 saved order 1,2,10 confirmed in M3U, track tags and processed metadata; move/undo controls inspected | PASS |
| Upload naming | Duplicate resolves to existing-book link; slug secondary; new chosen title appears as draft, then Save persists it | PASS |
| Upload success | Single picker uploads MP3, processing opens editor; existing-book append refreshes metadata and shows two chapters | PASS |
| Retry | Read-only fixture destination causes failure and Retry; after restoring writability retry succeeds with Uploaded and refreshed editor metadata | PASS |
| Upload edge cases | Empty file/duplicate filenames rejected before transfer; picker reusable; stale-check version guard and early upload lock reviewed | PASS |
| Format variants | MP3/M4A/ZIP, replacement/order/server-limit help visible; case-insensitive recognition reviewed; ZIP/M4A conversion not repeated in browser | INCONCLUSIVE |
| Sharing review | Mutual browse/listen/edit/delete permissions explained; invalid code returns error; guest resolved before connection | PASS |
| Sharing mutations | Connect/disconnect succeeds and refreshes session; database confirms mutual removal; regression confirms stale claims cannot retain access | PASS |
| Other sharing branches | Copy/self/already-connected paths present; every branch/clipboard content not independently exercised | INCONCLUSIVE |
| Playback | Mobile labels next to values; plain status names and explicit UTC; activity filter yields zero matches | PASS |
| Playback completion | Confirmed Mark finished succeeds and active record count becomes zero | PASS |
| Maintenance preview | Three descriptions inspected; preview lists exactly three stored fixtures across all libraries; cancellation does not run operation | PASS |
| Maintenance execution | Restore queued and completed for all three fixtures; each shows title, Completed and UTC time | PASS |
| Other maintenance branches | Reprocess/unzip reviews and busy guards inspected; those batch executions/running timing not independently exercised | INCONCLUSIVE |
| Processing failure | Regression verifies failure terminates successful extraction/persistence and reports a failed outcome | PASS |
| Language | New copy in neutral English resources; duration tests 698→11h 38m, 59→0h 59m, 1500→25h 0m and Unrated pass | PASS |
| Cleanup | Six fixture books/one playback removed from database; connection removed; generated files moved into ignored .aspire/ux-review-fixtures outside active storage; zero library titles verified; viewport reset | PASS |

No checklist items remain TODO or in progress. INCONCLUSIVE entries identify unexercised variants, not confirmed failures. No framework error overlay remained after validation.

## Automated checks

- DotCast.Library.Mcp.Tests: 53 passed, including search/access/save-failure/processing-failure regressions.
- DotCast.Infrastructure.ApiKeys.Tests: 6 passed.
- DotCast.Infrastructure.PresignedUrls.Tests: 3 passed.
- Total: 62 passed. Final frontend-only copy/order polish compiled successfully after these runs.
- Final command: dotnet build DotCast.App/DotCast.App.csproj --no-restore -v quiet.
- git diff --check: passed.

## Resolved defects

The old save path could update the library before a file write failed. The new save request writes/validates storage first, updates the library second and retains the draft on error. Failed processing no longer publishes success metadata. Library access now uses current sharing records so disconnected users cannot retain access through stale claims. A chosen upload title is retained for review/Save, and existing uploads refresh editor chapters while preserving newer dirty drafts.

## Screenshots and limits

Screenshots were captured and inspected inline; no standalone image files or paths are claimed. Visual checks covered mobile library, details/editor and admin playback cards at 390×844, and desktop grid/long-title wrapping at 1440×1000.

Not fully exercised: screen-reader traversal, device-specific podcast links, archive transfers, all conversion formats, delete execution, all network failures and the variants classified above. File writes/database changes are not one cross-system transaction; partial file writes can still require retry, though storage failure prevents the database update. New imports receive added dates; legacy unknown dates sort last. Processing history holds at most 200 recent book outcomes in the current process and resets on restart. No production changes or deployment occurred.
