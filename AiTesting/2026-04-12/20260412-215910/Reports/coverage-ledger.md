# DotCast Coverage Ledger

Environment used: Aspire AppHost, https://localhost:7062

Auth method used: seeded credentials from appsettings.json

Current authenticated user/role at end: guest

Top-level navigation items discovered:
- AudioBooks -> /
- admin -> /UserProfile
- Tichý Michal -> https://tichymichal.net
- HERE -> 
- 🗙 -> 

Visited top-level sections:
- AudioBooks
- UserProfile

Visited routes:
- /Login
- /Login?Message=Login+failed
- /
- /UserProfile
- /AudioBook/ja-poutnik-test/edit
- /api/logout
- /swagger
- /definitely-not-a-real-route

Visited secondary navigation:
- None

Dialogs/overlays opened:
- Suggestions modal
- alert: Link copied to clipboard!

Discovered during run checklist items:
- C026: Select first Suggestions result on edit page

State-changing flows completed:
- C005: Restore Audiobooks
- C010: Add category
- C011: Provided ZIP selected for upload
- C012: Sort chapters by name
- C013: Edit metadata and save

Destructive actions completed:
- None

Cleanup actions completed:
- None

Checklist item statuses:
- C001: PASS - Login form rendered with expected fields
- C002: PASS - Redirected to /Login
- C003: FAIL - `/Login?Message=Login+failed` loaded but no visible `Login failed` message was rendered on the login form
- C004: PASS - Admin login reached protected app
- C005: PASS - Restore flow completed sufficiently for exploration; 1 audiobook card(s) visible
- C006: FAIL - Statistics values are inconsistent with library content: UI showed `Titles: 0`, `Authors: 0`, `Duration: 0days 0 hours` while 1 audiobook card and author index entry were visible
- C007: PASS - Search accepted input without Blazor error
- C008: PASS - Edit form rendered with suggestions and upload controls
- C009: PASS - Suggestions modal opened
- C010: PASS - Selected category Architecture
- C011: PASS - Provided ZIP uploaded with HTTP 200; 1 progress element(s) created
- C012: PASS - Sort by name clicked without immediate UI error
- C013: PASS - Edited name persisted after save and reload
- C014: PASS - RSS action attempted; dialogs: alert: Link copied to clipboard!
- C015: BLOCKED - No distinct download button found
- C016: PASS - Profile and library sharing rendered for admin
- C017: PASS - Invalid share code did not crash the app
- C018: BLOCKED - Processing was already running or button disabled
- C019: BLOCKED - Processing was already running or button disabled
- C020: FAIL - Logout stayed on `/api/logout` and displayed the Blazor error overlay instead of reaching `/Login`
- C021: PASS - Guest login reached protected profile
- C022: FAIL - Admin Actions section visible to guest
- C023: BLOCKED - Route rendered app fallback rather than Swagger/OpenAPI UI
- C024: FAIL - Not-found text rendered, but the Blazor error overlay was visible on the unknown route
- C025: PASS - Mobile screenshots captured for login, library, and profile
- C026: FAIL - Suggestions list exposed 10 selectable results; selecting the first result changed name and author to `ERROR`, cleared description, and left order as `0`

Screenshot artifact index:
- See `screenshot-index.md`

Visual inspection notes:
- C001_login_page.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C002_logged_out_root_redirect.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C003_invalid_login.png: FAIL: invalid-login URL loaded, but the expected visible failure message was absent
- C004_admin_login_audio_books.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C016_admin_user_profile.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C005_restore_audiobooks_clicked.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C006_admin_library_page.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C007_library_search_poutnik.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C008_edit_first_book.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C009_suggestions_dialog.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C026_suggestions_followup.png: PASS/FAIL evidence: suggestions modal opened with 10 Select actions, but result data included invalid `ERROR` metadata
- C027_suggestion_selected_followup.png: FAIL: selecting the first suggestion populated the edit form with `ERROR` name and author and cleared the description
- C010_add_category.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C011_zip_upload_started.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C012_sort_chapters.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C013_save_edit_return_library.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C014_copy_rss_first_book.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C017_share_invalid_code.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C020_logout_to_login.png: FAIL: page stayed on `/api/logout` with the Blazor error overlay visible
- C021_guest_login_profile.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C022_guest_user_profile.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C023_swagger_route.png: BLOCKED/FAIL evidence: no Swagger UI; fallback rendered with Blazor error overlay visible
- C024_not_found_route.png: FAIL: not-found text rendered with Blazor error overlay visible
- C025a_mobile_login.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C025b_mobile_library.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect
- C025c_mobile_profile.png: PASS: page rendered; no obvious blank page, stale spinner, or overlay defect

Anomalies observed:
- FAIL: Invalid login failure message is not visible
- BLOCKED: Download action unavailable
- BLOCKED: Reprocess (No Unzip) disabled
- BLOCKED: Reprocess (Unzip) disabled
- FAIL: Logout route leaves Blazor error overlay instead of reaching Login
- FAIL: Guest can see Admin Actions
- FAIL: Library statistics show zero values while an audiobook is visible
- FAIL: Suggestions can overwrite metadata with ERROR values
- BLOCKED: Swagger not available
- FAIL: Unknown routes show the Blazor error overlay

Console notes:
- info: [2026-04-12T20:03:08.397Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=h0EwGf_xJW6shi3i-00GoA.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:03:12.148Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:03:12.155Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=xeIPabDE_cp7-OsaPn2j6Q.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- log: Hayes Terry - Já, Poutník (2020).zip uploaded successfully!
- info: [2026-04-12T20:04:24.990Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:24.995Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=L7cOxZcjGzEFQob8ojmIkQ.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:26.092Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:26.101Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=0dmtYUYwf_aUTwzocqHS0g.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:28.884Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:28.891Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=pCWCK59IVdWbScm2WGwYsQ.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:31.113Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:31.120Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=w_7zZlBWsfTD7OuuZr9dnQ.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- error: [2026-04-12T20:04:32.215Z] Error: System.InvalidOperationException: Authorization requires a cascading parameter of type Task<AuthenticationState>. Consider using CascadingAuthenticationState to supply this.
   at Microsoft.AspNetCore.Components.Authorization.AuthorizeViewCore.OnParametersSetAsync()
   at Microsoft.AspNetCore.Components.ComponentBase.CallStateHasChangedOnAsyncCompletion(Task task)
   at Microsoft.AspNetCore.Components.ComponentBase.RunInitAndSetParametersAsync()
- info: [2026-04-12T20:04:32.217Z] Information: Connection disconnected.
- info: [2026-04-12T20:04:43.331Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:43.337Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=bhFyxl6l8UIgWXzEOZi12A.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:44.574Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:44.580Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=xqtwnTE3rT5cs5vSe8iv8Q.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:45.648Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:45.653Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=cNAKDGONBmx2LRhchNT52Q.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:47.862Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:47.868Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=zRIm37iOLJrUV75RI-IShA.
- error: [2026-04-12T20:04:47.873Z] Error: System.InvalidOperationException: Authorization requires a cascading parameter of type Task<AuthenticationState>. Consider using CascadingAuthenticationState to supply this.
   at Microsoft.AspNetCore.Components.Authorization.AuthorizeViewCore.OnParametersSetAsync()
   at Microsoft.AspNetCore.Components.ComponentBase.CallStateHasChangedOnAsyncCompletion(Task task)
   at Microsoft.AspNetCore.Components.ComponentBase.RunInitAndSetParametersAsync()
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:49.517Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:49.522Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=ZzMyF4twpxAMJr_CXQawzw.
- error: [2026-04-12T20:04:49.530Z] Error: System.InvalidOperationException: Authorization requires a cascading parameter of type Task<AuthenticationState>. Consider using CascadingAuthenticationState to supply this.
   at Microsoft.AspNetCore.Components.Authorization.AuthorizeViewCore.OnParametersSetAsync()
   at Microsoft.AspNetCore.Components.ComponentBase.CallStateHasChangedOnAsyncCompletion(Task task)
   at Microsoft.AspNetCore.Components.ComponentBase.RunInitAndSetParametersAsync()
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:51.169Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:51.176Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=d9QM4YwaHfSlJ7DWjzNWMw.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:52.291Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:52.301Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=BGith-ZzRHUiezeg6AoZig.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:53.523Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:53.529Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=y7_POCSH9ELXxFRbpcX7hg.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:54.608Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:54.615Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=PYZ87g78RjghE0UtdWVLCQ.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.
- info: [2026-04-12T20:04:55.776Z] Information: Normalizing '_blazor' to 'https://localhost:7062/_blazor'.
- info: [2026-04-12T20:04:55.785Z] Information: WebSocket connected to wss://localhost:7062/_blazor?id=DSere1Z0PBvDIOX681HvaQ.
- log: %cThank you for using the free version of the Blazorise component library! We're happy to offer it to you for personal use. If you'd like to remove this message, consider purchasing a commercial license from https://blazorise.com/commercial. We appreciate your support! [color: #3B82F6; padding: 0;]
- debug: CSS Hot Reload ignoring https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://use.fontawesome.com/releases/v5.15.4/css/all.css because it was inaccessible or had more than 7000 rules.
- debug: CSS Hot Reload ignoring https://localhost:7062/_content/Blazorise.Bootstrap/blazorise.bootstrap.css because it was inaccessible or had more than 7000 rules.

HTTP errors observed:
- None

Blockers:
- Download action unavailable: No distinct download button found
- Reprocess (No Unzip) disabled: Processing was already running or button disabled
- Reprocess (Unzip) disabled: Processing was already running or button disabled
- Swagger not available: Route rendered app fallback rather than Swagger/OpenAPI UI

Remaining queue:
- None
