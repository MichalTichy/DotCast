# Major Prototype Results

The first screenshot pass was too subtle, so these results supersede the earlier CSS-only prototype folders. For this pass, each prototype changed page structure in the Razor components, was rebuilt through Aspire, captured with Playwright, and then reverted before the next prototype.

## Reference

- User-provided Overseerr screenshot: dark side navigation, poster-first discovery, dense horizontal media rails, compact badges, and quick browsing over heavy forms.
- Captured local Overseerr reference: `redesign/screenshots/reference/overseerr-login-or-home.png`.

## 1. Cinematic Discovery

Document: `redesign/philosophy-01-cinematic-discovery.md`

Structural direction:

- Replaced the author index layout with a media-app shell.
- Added a persistent navigation rail, top discovery bar, hero audiobook, category chips, and horizontal author rails.
- Reworked the edit page around cover-first metadata panels instead of stacked form rows.
- Reworked profile into account, sharing, and admin panels with stronger hierarchy.

Screenshots:

- `redesign/screenshots/major-01-cinematic-discovery/library-desktop.png`
- `redesign/screenshots/major-01-cinematic-discovery/edit-desktop.png`
- `redesign/screenshots/major-01-cinematic-discovery/profile-desktop.png`
- `redesign/screenshots/major-01-cinematic-discovery/library-mobile.png`

## 2. Librarian Workbench

Document: `redesign/philosophy-02-librarian-workbench.md`

Structural direction:

- Replaced cover-card browsing with a table-like operational catalogue.
- Added a filter column, dense metadata rows, thumbnail covers, and explicit action columns.
- Rebuilt the edit page as a two-column metadata workbench with side facts and grouped editing panels.
- Rebuilt profile as an operations console for sharing and maintenance.

Screenshots:

- `redesign/screenshots/major-02-librarian-workbench/library-desktop.png`
- `redesign/screenshots/major-02-librarian-workbench/edit-desktop.png`
- `redesign/screenshots/major-02-librarian-workbench/profile-desktop.png`
- `redesign/screenshots/major-02-librarian-workbench/library-mobile.png`

## 3. Calm Editorial

Document: `redesign/philosophy-03-calm-editorial.md`

Structural direction:

- Replaced the card wall with an editorial desk: large title area, search and statistics panel, featured book, author index, and long-form author sections.
- Rebuilt book rows as readable editorial list items with cover, summary, categories, rating, and actions.
- Rebuilt the edit page as a numbered workflow: identity, description, categories, files, chapters.
- Rebuilt profile as settings navigation with account, sharing, and maintenance sections.

Screenshots:

- `redesign/screenshots/major-03-calm-editorial/library-desktop.png`
- `redesign/screenshots/major-03-calm-editorial/edit-desktop.png`
- `redesign/screenshots/major-03-calm-editorial/profile-desktop.png`
- `redesign/screenshots/major-03-calm-editorial/library-mobile.png`

## Revert Status

All temporary prototype changes to the app were reverted after screenshot capture. The tracked app files have no content diff from `HEAD`; the remaining outputs are the `redesign` documents and screenshots.

Earlier folders `redesign/screenshots/01-*`, `02-*`, and `03-*` are from the rejected subtle CSS pass. Use the `major-*` folders for redesign review.
