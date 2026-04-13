# DotCast Exploratory Checklist

Environment: Aspire running AppHost `DotCast.AppHost/DotCast.AppHost.csproj`, web app `https://localhost:7062`

Known auth seed: `admin/admin`, `guest/guest`

Source coverage notes:
- ADO requirements source: BLOCKED. Git remote points to GitHub, no Azure DevOps project metadata was available from local git settings.
- Usecase classes: none found by `rg "class .*Usecase"`.
- Swagger: TODO. Source does not configure Swagger; verify `/swagger` in browser/API.
- Pages from source: `/Login`, `/`, `/UserProfile`, `/AudioBook/{Id}/edit` when an audio book exists.
- API endpoints from source: `POST /api/login`, `GET /api/logout`, `PUT /storage/archive/{AudioBookId}`, `PUT /storage/file/{AudioBookId}/{FileId}`, `GET /storage/archive/{AudioBookId}`, `GET /storage/file/{AudioBookId}/{FileId}`, `GET /library/{AudioBookId}/{UserId}/rss`.

## Initial Checklist

| ID | Status | Parent Flow | Action | Type | Expected observable outcome | Evidence |
|---|---|---|---|---|---|---|
| C001 | TODO | Public access | Navigate to `/Login` | Visual/page | Login page renders with username, password, remember me, login action | Screenshot |
| C002 | TODO | Auth | Submit invalid login | Edge | User remains on login and sees `Login failed` or equivalent error | Screenshot, URL |
| C003 | TODO | Auth | Submit admin login | Happy path | Login form disappears, app chrome and protected AudioBooks page render | Screenshot, URL, cookie |
| C004 | TODO | Auth | Submit guest login | Role variant | Guest can authenticate and protected chrome renders | Screenshot, URL |
| C005 | TODO | Protected routing | Visit `/` logged out | Edge | Redirects to `/Login` | Screenshot, URL |
| C006 | TODO | Library | Visit `/` as admin | Visual/page | AudioBooks page renders stats, author index/search, and any cards or empty state | Screenshot |
| C007 | TODO | Library | Search library | Interaction | Search field accepts query and page updates or remains stable with no error | Screenshot |
| C008 | TODO | Library | If audio book card exists, copy RSS | Interaction/API | Copy alert or alternate RSS URL open occurs without error | Dialog/screenshot |
| C009 | TODO | Library | If audio book card exists, download archive | Interaction/API | Download/open attempts without UI error | Browser/network note |
| C010 | TODO | Library | If audio book card exists, open edit page | Navigation | `/AudioBook/{id}/edit` renders edit form | Screenshot |
| C011 | TODO | Edit audio book | Open suggestions dialog | Dialog/API | Suggestions modal opens or a clear failure/blocker is visible | Screenshot |
| C012 | TODO | Edit audio book | Edit metadata and save | State-changing | Save returns to `/` and changed value persists after revisit | Screenshot |
| C013 | TODO | Edit audio book | Add/remove category | State-changing | Category list changes and missing-category select updates | Screenshot |
| C014 | TODO | Edit audio book | Upload provided zip | State-changing/upload | File input accepts provided zip, progress appears, upload finishes or failure is captured | Screenshot, network/logs |
| C015 | TODO | Edit audio book | Sort chapters by name | State-changing | Chapter order changes or button is blocked by no chapters | Screenshot |
| C016 | TODO | Profile | Visit `/UserProfile` as admin | Visual/page | User profile, library code, sharing controls, admin actions render | Screenshot |
| C017 | TODO | Profile | Share invalid library code | Edge | UI stays stable, no crash; error handling is observable or absent | Screenshot |
| C018 | TODO | Profile | Admin restore audiobooks | State-changing | Button becomes disabled or processing state starts, no immediate app error | Screenshot/log |
| C019 | TODO | Profile | Admin reprocess without unzip | State-changing | Button becomes disabled or processing state starts, no immediate app error | Screenshot/log |
| C020 | TODO | Profile | Admin reprocess with unzip | State-changing | Button becomes disabled or processing state starts, no immediate app error | Screenshot/log |
| C021 | TODO | Auth | Logout | Happy path | User reaches `/Login`; protected page access redirects | Screenshot |
| C022 | TODO | Role/authorization | Visit `/UserProfile` as guest | Role variant | Guest profile renders; admin actions should not be visible | Screenshot |
| C023 | TODO | Swagger/API inventory | Visit `/swagger` | API docs | Swagger UI appears or app fallback/404 proves unavailable | Screenshot |
| C024 | TODO | Not found | Visit unknown route | Edge/page | Not-found message renders inside app layout | Screenshot |
| C025 | TODO | Responsive | Capture mobile screenshots of key pages | Visual | Pages remain usable without clipped controls | Screenshots |

