# Fix Verification

## Fixed and verified

- Invalid login message
  - Fix: `LoginEndpoint` redirects failed logins to `/Login/{Message}` and `Login.razor` renders the message in plain alert markup.
  - Verification: Playwright invalid-login flow landed on `/Login/Login%20failed` and `document.body.innerText` contained `Login failed`.

- Library statistics
  - Fix: `AudioBookStatisticSpecification` now loads the visible audiobook documents and computes count, author count, and duration in memory instead of projecting the computed `AudioBookInfo.Duration` property inside the Marten query.
  - Verification: Playwright admin library check returned `Titles: 1`, `Authors: 1`, non-zero duration, and `cards=1`.

- Suggestions overwriting metadata with `ERROR`
  - Fix: `AudiobookInfoSuggestionsRequestHandler` filters invalid provider results, `AudioBookEdit.Prefill` refuses invalid suggestions, and the modal renders `No valid suggestions found.` when nothing valid remains.
  - Verification: Playwright edit-page check found no `ERROR` text, zero `Select` buttons, the empty-state text, and unchanged name/author fields.

- Guest Admin Actions visibility
  - Fix: `UserProfile` now sets `IsAdmin` from the current user's role claims instead of comparing the admin role constant to itself.
  - Verification: Playwright guest profile check returned `Admin Actions` absent.

- Logout route
  - Fix: Logout button uses `NavigationManager.NavigateTo(..., forceLoad: true)` so `/api/logout` is handled by the server endpoint.
  - Verification: Playwright logout check landed on `/Login` with `#blazor-error-ui` hidden.

- Unknown-route Blazor error overlay
  - Fix: `App.razor` wraps the router in `CascadingAuthenticationState`, so layout `AuthorizeView` has authentication state even in the `NotFound` branch.
  - Verification: Playwright unknown-route check found the not-found text and `#blazor-error-ui` hidden.

- Download action discoverability
  - Fix: The icon-only download button now includes hidden text `Download audiobook`.
  - Verification: Playwright library check found one named download button and one download icon button.

- Reprocess availability
  - Fix: No code change required; the previous blocker was active processing state.
  - Verification: Playwright admin profile check found both `#reprocess-no-unzip` and `#reprocess-unzip` enabled while processing was idle.

## Not changed

- Swagger was intentionally not added.

## Other validation

- Aspire app resource: healthy.
- Aspire console logs: empty.
- Aspire structured logs: empty.
- `dotnet test DotCast.Infrastructure.PresignedUrls.Tests\DotCast.Infrastructure.PresignedUrls.Tests.csproj --no-build`: 3 passed.
