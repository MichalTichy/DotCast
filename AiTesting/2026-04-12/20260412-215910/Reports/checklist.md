# DotCast Exploratory Checklist

Environment: Aspire running AppHost `DotCast.AppHost/DotCast.AppHost.csproj`, web app `https://localhost:7062`

Known auth seed: `admin/admin`, `guest/guest`

Test audiobook fixture: `C:\Users\tichy\Downloads\Hayes Terry - Ja, Poutnik (2020)\Hayes Terry - Ja, Poutnik (2020).zip`; current Aspire storage also contains extracted fixture `D:\Work\DotCast\.aspire\audiobooks\ja-poutnik-test`.

Source coverage notes:
- ADO requirements source: BLOCKED. Git remote points to GitHub, no Azure DevOps project metadata was available from local git settings.
- Usecase classes: none found by source search for `Usecase`/`UseCase`.
- Swagger: source does not configure Swagger; verify `/swagger` at runtime.
- Pages from source: `/Login`, `/`, `/UserProfile`, `/AudioBook/{Id}/edit` when an audio book exists.
- API endpoints from source: `POST /api/login`, `GET /api/logout`, `PUT /storage/archive/{AudioBookId}`, `PUT /storage/file/{AudioBookId}/{FileId}`, `GET /storage/archive/{AudioBookId}`, `GET /storage/file/{AudioBookId}/{FileId}`, `GET /library/{AudioBookId}/{UserId}/rss`.

| ID | Status | Parent Flow | Action | Type | Expected observable outcome | Evidence |
|---|---|---|---|---|---|---|
| C001 | TODO | Public access | Navigate to `/Login` | Visual/page | Login page renders username, password, remember-me, and login action | Screenshot, URL |
| C002 | TODO | Protected routing | Visit `/` logged out | Edge | Redirects to `/Login` | Screenshot, URL |
| C003 | TODO | Auth | Submit invalid login | Edge | User remains on login and sees `Login failed` or equivalent error | Screenshot, URL |
| C004 | TODO | Auth | Submit admin login | Happy path | Login form disappears; protected app chrome and AudioBooks page render | Screenshot, URL |
| C005 | TODO | Fixture restore | Admin restore audiobooks | State-changing | Fixture from Aspire storage appears in library or processing starts without crash | Screenshot, app state |
| C006 | TODO | Library | Visit `/` as admin | Visual/page | AudioBooks page renders stats, author index/search, and audiobook cards or clear empty state | Screenshot |
| C007 | TODO | Library | Search library for `Poutnik` | Interaction | Search field accepts query and app remains stable | Screenshot |
| C008 | TODO | Library | Open first audiobook edit page | Navigation | `/AudioBook/{id}/edit` renders edit form | Screenshot |
| C009 | TODO | Edit audio book | Open suggestions dialog | Dialog/API | Suggestions modal opens or failure/blocker is captured | Screenshot |
| C010 | TODO | Edit audio book | Add category | State-changing | Category list changes and missing-category select updates | Screenshot |
| C011 | TODO | Edit audio book | Upload provided audiobook ZIP | State-changing/upload | File input accepts provided ZIP and progress or network result is captured | Screenshot, network/logs |
| C012 | TODO | Edit audio book | Sort chapters by name | State-changing | Chapter list remains stable and no app error appears | Screenshot |
| C013 | TODO | Edit audio book | Edit metadata and save | State-changing | Save returns to `/` and edited title persists after reload/revisit | Screenshot |
| C014 | TODO | Library | Copy RSS link for first card | Interaction/API | Copy dialog or alternate RSS behavior occurs without app crash | Dialog/screenshot |
| C015 | TODO | Library | Download archive for first card | Interaction/API | Download/open attempts without UI error | Browser/network note |
| C016 | TODO | Profile | Visit `/UserProfile` as admin | Visual/page | User profile, library code, sharing controls, admin actions render | Screenshot |
| C017 | TODO | Profile | Share invalid library code | Edge | UI stays stable, no crash | Screenshot |
| C018 | TODO | Profile | Admin reprocess without unzip | State-changing | Button action is accepted or blocked by processing state without crash | Screenshot/log |
| C019 | TODO | Profile | Admin reprocess with unzip | State-changing | Button action is accepted or blocked by processing state without crash | Screenshot/log |
| C020 | TODO | Auth | Logout | Happy path | User reaches `/Login`; protected access redirects | Screenshot |
| C021 | TODO | Auth | Submit guest login | Role variant | Guest can authenticate and reach protected profile | Screenshot |
| C022 | TODO | Role/authorization | Visit `/UserProfile` as guest | Role variant | Guest profile renders; admin actions should not be visible | Screenshot |
| C023 | TODO | Swagger/API inventory | Visit `/swagger` | API docs | Swagger UI appears, or fallback/404 proves unavailable | Screenshot |
| C024 | TODO | Not found | Visit unknown route | Edge/page | Not-found message or fallback behavior is documented | Screenshot |
| C025 | TODO | Responsive | Capture mobile screenshots of login, library, profile | Visual | Pages remain usable without clipped primary controls | Screenshots |
