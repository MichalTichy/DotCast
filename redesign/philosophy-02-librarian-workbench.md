# Philosophy 02: Librarian Workbench

## Summary

This approach treats DotCast as a high-efficiency library management tool. It favors density, scanning, sorting, filtering, metadata health, and operational clarity over cinematic presentation.

## Core Idea

The user is managing a collection. The interface should make it easy to see what exists, what needs attention, what is available to download or subscribe to, and what processing/admin work is happening.

## Why It Fits DotCast

The current app already exposes many management tasks:

- restore audiobooks
- reprocess files
- edit metadata
- sort chapters
- upload archives
- copy RSS feeds
- manage library sharing

A workbench design accepts that DotCast is partly an operations tool and makes those workflows explicit instead of hiding them inside cards and profile pages.

## Design Principles

- Prefer lists, tables, split panes, and compact rows for management-heavy screens.
- Use clear status indicators for metadata completeness, file availability, duration, RSS, and processing state.
- Keep filters persistent and predictable.
- Make bulk actions and batch review possible.
- Reduce vertical waste.
- Every long-running action should show status, history, and failure recovery.

## Library Page Direction

- Replace large cards with a compact list or table-like collection view.
- Keep cover thumbnails small but visible.
- Put author, series, duration, rating, categories, and action buttons in consistent columns.
- Add metadata status and processing status.
- Make search, author filter, category filter, and sort controls obvious.

## Edit Page Direction

- Use a two-pane editor: metadata form on one side, chapters/files/status on the other.
- Show validation and missing metadata inline.
- Make chapter operations compact and batch-friendly.

## Risks

- Can feel less emotional or less media-like.
- Requires careful visual design to avoid becoming a dense admin dashboard.
- Best for power users; may feel too utilitarian for casual browsing.

## Best Use

This is strongest if DotCast should prioritize collection maintenance, correctness, and admin speed.
