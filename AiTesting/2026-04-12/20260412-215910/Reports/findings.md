# Findings

## FAIL

- Invalid login failure message is not visible
  - Route: https://localhost:7062/Login?Message=Login+failed
  - Evidence: The invalid login redirects to `/Login?Message=Login+failed`, but the screenshot and visible text show only the blank login form with no `Login failed` message.
- Logout route leaves Blazor error overlay instead of reaching Login
  - Route: https://localhost:7062/api/logout
  - Evidence: Clicking Logout stayed on `/api/logout` and displayed the Blazor error UI overlay instead of navigating to `/Login`.
- Library statistics show zero values while an audiobook is visible
  - Route: https://localhost:7062/
  - Evidence: Follow-up check returned stats `Titles: 0`, `Authors: 0`, `Duration: 0days 0 hours` while the same page had 1 `.c-card` and an author index entry `Martin Stránský ,Terry Hayes`.
- Guest can see Admin Actions
  - Route: /UserProfile
  - Evidence: Admin Actions section visible to guest
- Suggestions can overwrite metadata with ERROR values
  - Route: https://localhost:7062/AudioBook/ja-poutnik-test/edit
  - Evidence: Follow-up suggestions check found 10 suggestion cards and 10 Select buttons. Selecting the first suggestion changed the edit fields from `Já, Poutník` / `Martin Stránský ,Terry Hayes` to `ERROR` / `ERROR`, cleared the description, and left order as `0`.
  - Code pointer: `DotCast.Infrastructure.BookInfoProvider.DatabazeKnih/DatabazeKnihBookInfoProvider.cs` returns `FoundBookInfo(title ?? "ERROR", author ?? "ERROR", ...)`; `DotCast.App/Pages/AudioBookEdit.razor.cs` then copies suggestion fields directly in `Prefill`.
- Unknown routes show the Blazor error overlay
  - Route: https://localhost:7062/definitely-not-a-real-route
  - Evidence: The not-found text rendered, but the Blazor error UI overlay was visible.

## INCONCLUSIVE

None.

## BLOCKED

- Download action unavailable
  - Route: https://localhost:7062/
  - Evidence: No distinct download button found
- Reprocess (No Unzip) disabled
  - Route: https://localhost:7062/UserProfile
  - Evidence: Processing was already running or button disabled
- Reprocess (Unzip) disabled
  - Route: https://localhost:7062/UserProfile
  - Evidence: Processing was already running or button disabled
- Swagger not available
  - Route: /swagger
  - Evidence: Route rendered app fallback rather than Swagger/OpenAPI UI; the fallback page also showed the Blazor error overlay.
