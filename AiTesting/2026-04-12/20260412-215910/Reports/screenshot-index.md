# Screenshot Index

| File | Trigger | Route | Visual verdict |
|---|---|---|---|
| C001_login_page.png | login_page | /Login | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C002_logged_out_root_redirect.png | logged_out_root_redirect | /Login | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C003_invalid_login.png | invalid_login | /Login?Message=Login+failed | FAIL: invalid-login URL loaded, but the expected visible failure message was absent |
| C004_admin_login_audio_books.png | admin_login_audio_books | / | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C016_admin_user_profile.png | admin_user_profile | /UserProfile | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C005_restore_audiobooks_clicked.png | restore_audiobooks_clicked | /UserProfile | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C006_admin_library_page.png | admin_library_page | / | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C007_library_search_poutnik.png | library_search_poutnik | / | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C008_edit_first_book.png | edit_first_book | /AudioBook/ja-poutnik-test/edit | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C009_suggestions_dialog.png | suggestions_dialog | /AudioBook/ja-poutnik-test/edit | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C026_suggestions_followup.png | suggestions_followup | /AudioBook/ja-poutnik-test/edit | PASS/FAIL evidence: suggestions modal opened with 10 Select actions, but result data included invalid `ERROR` metadata |
| C027_suggestion_selected_followup.png | suggestion_selected_followup | /AudioBook/ja-poutnik-test/edit | FAIL: selecting the first suggestion populated the edit form with `ERROR` name and author and cleared the description |
| C010_add_category.png | add_category | /AudioBook/ja-poutnik-test/edit | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C011_zip_upload_started.png | zip_upload_started | /AudioBook/ja-poutnik-test/edit | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C012_sort_chapters.png | sort_chapters | /AudioBook/ja-poutnik-test/edit | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C013_save_edit_return_library.png | save_edit_return_library | / | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C014_copy_rss_first_book.png | copy_rss_first_book | / | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C017_share_invalid_code.png | share_invalid_code | /UserProfile | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C020_logout_to_login.png | logout_to_login | /api/logout | FAIL: page stayed on `/api/logout` with the Blazor error overlay visible |
| C021_guest_login_profile.png | guest_login_profile | /UserProfile | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C022_guest_user_profile.png | guest_user_profile | /UserProfile | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C023_swagger_route.png | swagger_route | /swagger | BLOCKED/FAIL evidence: no Swagger UI; fallback rendered with Blazor error overlay visible |
| C024_not_found_route.png | not_found_route | /definitely-not-a-real-route | FAIL: not-found text rendered with Blazor error overlay visible |
| C025a_mobile_login.png | mobile_login | /Login | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C025b_mobile_library.png | mobile_library | / | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
| C025c_mobile_profile.png | mobile_profile | /UserProfile | PASS: page rendered; no obvious blank page, stale spinner, or overlay defect |
