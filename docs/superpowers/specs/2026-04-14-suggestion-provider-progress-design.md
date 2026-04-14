# Suggestion Provider Progress Design

## Goal

Improve the audiobook metadata suggestion flow so users can see provider progress, compare ranked suggestions, and apply fields without accidentally wiping useful metadata.

## Scope

This change covers the edit page Suggestions tab, the suggestion request/response shape used by that page, and relevance scoring for returned suggestions. It does not change provider scraping behavior beyond wrapping provider failures and scoring valid results.

## Backend

Add provider-aware suggestion messages for the edit page:

- one request that returns available provider names
- one request that runs a single named provider and returns its suggestions

Keep the existing bulk suggestion request behavior available for callers that only need a flat list. The edit page should use the provider-aware request shape so it can start one task per provider and update each provider row independently.

Provider failures should not fail the whole search. A failed provider task is reported with failed status and an error message in the component, while successful providers still contribute suggestions.

## Relevance Score

Each `FoundBookInfo` includes a `RelevanceScore` value. The score is deterministic and based on the current audiobook search terms:

- strong title match
- partial title match
- strong author match
- partial author match
- matching or present series data
- useful metadata completeness such as description, image, categories, and rating

The final suggestion list is merged across completed providers and ordered by `RelevanceScore` descending. The UI displays the score on each suggestion.

## UI

The Suggestions tab shows provider status rows above the suggestion list. Each row has the provider name and a badge:

- spinner while that provider is running
- green tick when done
- red failed badge when failed

The suggestion list remains usable as providers finish. Suggestions are resorted by relevance whenever new provider results arrive.

## Apply Fields UX

Replace the raw checkbox stack with a clearer comparison table. Each row shows:

- field name
- apply toggle
- current value
- suggested value

Rows with no useful suggested value default to off and clearly say that the provider has no value for that field. Applying selected fields should not overwrite existing metadata with blank strings, empty category lists, or zero ratings.

The explicit apply action stays. After applying, show a short status message and keep the user on the Suggestions tab so they can review the changed fields before saving.

## Testing

Add focused unit coverage for relevance scoring and provider failure aggregation if the current test structure makes that practical. Verify the Blazor app builds. If a local app run is available, manually check:

- provider badges move through loading, done, and failed states
- failed providers do not block successful suggestions
- suggestions display scores and sort descending
- applying selected fields does not clear fields with empty provider values
