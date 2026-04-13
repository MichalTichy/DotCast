# 04 Quick Detail Drawer

Goal: make poster/card click open a quick audiobook detail view.

- Poster/card primary click opens a Blazor-controlled drawer on desktop.
- On mobile, use the same content as a bottom-sheet-style panel.
- Include cover, title, author, series, rating, duration, categories, description, Copy RSS, Download, Edit, and close.
- Keep edit as a secondary action.

Acceptance:

- Drawer opens from every library card.
- RSS copy and download actions still work.
- Edit navigates to `/AudioBook/{Id}/edit`.
- Drawer is closable and keyboard reachable.
