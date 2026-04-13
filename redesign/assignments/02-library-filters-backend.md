# 02 Library Filters Backend

Goal: replace text-only library search with structured backend filters.

- Add a shared filter model with search text, authors, categories, series, min rating, and max rating.
- Update `AudioBooksRetrievalRequest` to carry the structured filter.
- Update the retrieval specification to apply filters server-side.
- Add a facets request/result for available authors, categories, and series.
- Preserve current library access behavior.

Acceptance:

- Existing search still works.
- Multi-select author/category/series filters work.
- Rating range works.
- Combined filters use AND across filter groups and OR within each group.
