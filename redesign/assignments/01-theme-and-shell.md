# 01 Theme And Shell

Goal: establish the global cinematic visual foundation.

- Add a Blazorise `ThemeProvider` with a warm dark theme.
- Replace the existing top navbar with a responsive global app shell.
- Desktop shell uses fixed left navigation.
- Mobile shell uses a compact top bar.
- Navigation items: Library, New Audiobook, Account, and admin-only Admin.
- Keep route behavior unchanged.

Acceptance:

- All authenticated pages render inside the new shell.
- Admin nav item is hidden for non-admin users.
- Theme applies to Blazorise controls.
- App builds and Aspire rebuild succeeds.
