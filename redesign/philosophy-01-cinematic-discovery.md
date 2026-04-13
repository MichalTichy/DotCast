# Philosophy 01: Cinematic Discovery

## Summary

This approach treats DotCast like a media discovery product. The interface should feel close to modern streaming request and media browsing tools, especially Overseerr: dark, immersive, cover-led, and optimized for quickly recognizing media from artwork.

The provided Overseerr screenshot shows the relevant pattern: a dark app shell, persistent left navigation, search at the top, dense horizontal media rails, poster-first cards, compact status badges, and browse rows for genres, studios, popular content, and upcoming content.

## Core Idea

Audiobooks are media objects first, records second. Covers, title, author, duration, and primary actions should dominate the library experience. Administrative details and metadata management should remain available, but they should not be the first thing the user sees.

## Why It Fits DotCast

DotCast is a personal audiobook server. Users likely return to:

- find an audiobook
- copy RSS or download it
- check metadata quality
- add or repair imported titles

A cinematic media-library design makes browsing feel natural and gives the app a stronger identity.

## Design Principles

- Use dark surfaces with high-contrast text and one restrained accent color.
- Put cover art at the center of recognition and navigation.
- Use poster/card grids for browsing, with clear hover/focus action zones.
- Keep metadata visible but secondary to the cover, title, author, and listening actions.
- Use overlays, drawers, and focused panels instead of long technical pages.
- Make empty states feel like media-library states, not blank application pages.

## Library Page Direction

- Replace the persistent author sidebar with a filter drawer.
- Use a responsive cover grid.
- Put search and filters in a compact top toolbar.
- Show stats as small secondary chips, not giant header content.
- Use strong card actions: play/open, RSS, download, edit.

## Edit Page Direction

- Use a dark detail page with the cover and key identity fields at the top.
- Move suggestions into a media-match workflow.
- Show chapters as a compact list or drawer, not hundreds of large cards.

## Risks

- Dark cinematic styling can reduce productivity for metadata-heavy workflows if overused.
- If covers are missing or low quality, the interface may feel weaker.
- Needs strong responsive behavior to avoid cramped mobile layouts.

## Best Use

This is strongest if DotCast should feel like a polished personal audiobook streaming/library product.
