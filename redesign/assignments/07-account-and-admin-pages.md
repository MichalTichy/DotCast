# 07 Account And Admin Pages

Goal: separate account/sharing from maintenance.

- Rebuild `/UserProfile` as an Account page with username, logout, library code, sharing, shared libraries, and unshare.
- Add `/Admin` UI for restore, reprocess, processing status, disabled/running state, and confirmations.
- Remove upload/admin maintenance UI from profile.

Acceptance:

- Sharing and unsharing still work.
- Invalid share code has visible feedback.
- Admin actions are admin-only.
- Maintenance buttons disable while processing is running.
