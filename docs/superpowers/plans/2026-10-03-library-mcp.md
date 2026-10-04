# Simplified library MCP implementation

This plan supersedes the original scoped-token implementation after the user's request for a simple API key.

- [x] Remove scopes, permission choices, expiry, names and token lists.
- [x] Remove the token-management controller, browser fetch module and separate token project.
- [x] Add one account-keyed digest document, atomic generate/replace and own-account revoke operations.
- [x] Keep account key storage, management and authentication in `DotCast.Infrastructure.ApiKeys`; MCP consumes the scheme and uses existing user/library access.
- [x] Reduce the profile to generate/regenerate/revoke and one-time key display.
- [x] Test key lifecycle, account boundaries, the four tools and request limits.
- [x] Verify the simplified profile in a disposable database and update the PR.

Integration tests require Docker. Client setup: `docs/library-mcp.md`.
