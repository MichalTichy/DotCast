# DotCast library MCP with an API key

Status: simplified and approved by the user on 2026-10-04; replaces the earlier scoped-token design.

- One random API key per account, with no scopes, name, expiry or list of tokens.
- Generate/regenerate/revoke directly from the existing authenticated Blazor profile through application handlers. No new controller or JavaScript bridge.
- Persist one Marten document keyed by account ID with only a SHA-256 key digest and creation time. Generating a key atomically replaces the previous one; revoking deletes it.
- Keep key storage, management and authentication in the independent `DotCast.Infrastructure.ApiKeys` capability. MCP consumes its authentication scheme.
- Authenticate the owner on every MCP HTTP request and use existing account claims and library permissions.
- Expose search, book details, metadata suggestions and a validated metadata patch. Reads cover accessible libraries; writes stay in the owner's library.
- Keep HTTPS, host validation, bounded input and request limits. Preserve cancellation and concurrent metadata correctness.
- Use infrastructure repositories for writes: `GetAndUpdateAsync` for metadata and atomic `UpsertAsync` for account keys. No MCP-specific persistence adapter or retry loop.
- Verify key replacement/revocation, account access and all four tools through real HTTP/PostgreSQL tests, then test the profile in an isolated database.
- Client setup and operational details live in `docs/library-mcp.md`.
