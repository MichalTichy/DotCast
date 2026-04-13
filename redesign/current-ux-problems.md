# Current UX Problems

This summarizes the main UX problems found from the exploratory screenshots in `AiTesting/2026-04-12/20260412-215910/Artifacts/Screenshots`.

## 1. Mobile Library Layout Breaks Down

The mobile library view is the biggest usability issue. The author sidebar remains visible and consumes about half the screen, while the main audiobook card is squeezed into the remaining space. The stats wrap awkwardly, and the selected author heading floats above the card.

Impact: the core browse experience is hard to use on phones.

Direction: replace the persistent sidebar with a filter drawer or collapsible filter panel on mobile. Use a single-column audiobook list or card layout with stable spacing.

## 2. Library Page Has Weak Information Hierarchy

The library view gives similar visual weight to the sidebar, stats, search field, author grouping, and audiobook card. The actual audiobook content does not clearly dominate the page. With only one item, the card floats in a large empty space.

Impact: users must work too hard to understand where to look and what actions are available.

Direction: make the audiobook collection the primary surface. Search, filters, and stats should support the collection, not compete with it.

## 3. Search Empty State Is Silent

When search returns no visible results, the page becomes mostly blank. There is no message explaining that no audiobooks matched, no reset action, and no indication that accent-insensitive or typo-tolerant search may be required.

Impact: users cannot tell whether search failed, data is missing, or the app is still loading.

Direction: add explicit empty states with active filters, a clear reset action, and useful guidance.

## 4. Edit Page Is an Unstructured Data Dump

The audiobook edit page combines metadata, cover art, categories, upload, suggestions, save, chapter sorting, and a very long chapter list on one screen. The chapters dominate the page and bury the metadata editing task.

Impact: editing metadata feels noisy and fragile, especially for large audiobooks.

Direction: split edit into focused sections or tabs: metadata, files, chapters, and publishing/RSS. Keep save/status visible and make the chapter list collapsible or virtualized.

## 5. Suggestions Are Not a Safe Metadata Import Flow

The suggestions overlay renders provider results as small cards while the full edit page and chapter list remain visible below. It is not clear what selecting a suggestion will overwrite, and there is no field-by-field preview.

Impact: applying suggestions feels risky and hard to compare.

Direction: redesign suggestions as a comparison workflow. Show current metadata beside the provider result, allow field-level selection, and use an explicit apply action.

## 6. User Feedback Is Weak or Missing

Invalid login, invalid library sharing, upload, copy RSS, and admin actions do not consistently provide strong feedback. Disabled admin buttons appear without clear status messaging.

Impact: users cannot reliably tell whether an action succeeded, failed, or is still processing.

Direction: define shared feedback patterns for validation errors, success confirmations, background jobs, and failed operations.

## 7. Error Routes Expose Framework-Level UI

The not-found route shows a Blazor-style error overlay with a refresh prompt. A missing page looks like an application crash.

Impact: users lose trust and have no useful recovery path.

Direction: add app-level error pages for not found, unauthorized, server error, and disconnected states.

## 8. Profile Mixes Unrelated Responsibilities

The profile page combines account identity, library sharing, and admin maintenance actions. Admin operations appear as plain buttons without context, confirmation, or job status.

Impact: the page is confusing and admin actions feel unsafe.

Direction: separate account, sharing, and admin areas. Admin actions should have descriptions, confirmations, and visible job progress/history.

## 9. The UI Exposes Implementation Details

Several screens expose internal structure directly: raw author buckets, raw chapter rows, raw file input, maintenance commands, and framework error states.

Impact: the product feels like a technical control panel rather than an audiobook library manager.

Direction: redesign around user tasks: browse audiobooks, edit metadata, manage files and chapters, share/subscribe, and administer the library.

## Highest-Priority Redesign Work

1. Redesign the app shell and responsive layout.
2. Rebuild the library page as the primary user surface.
3. Turn audiobook edit into a task-based workflow.
4. Replace suggestions with a safe metadata import flow.
5. Add consistent feedback and empty-state patterns.
6. Move admin operations into a dedicated admin area.
