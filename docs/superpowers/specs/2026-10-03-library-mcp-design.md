# DotCast library MCP with personal API tokens

Date: 2026-10-03
Status: proposed specification for user review; implementation has not started.

## Purpose and agreed direction

Allow an AI client to search an audiobook library and maintain its metadata through DotCast. The user selected local clients such as Codex or Claude Desktop, required normal authentication and authorization, and selected personal API tokens instead of OAuth.

Host the MCP endpoint in the running DotCast application over Streamable HTTP at `/mcp`. A local client connects to that endpoint with a personal token. Client location does not bypass authentication. This feature does not require direct database access from the client.

## Alternatives considered

1. An authenticated HTTP endpoint in DotCast: selected because it reuses the running application's identity, library operations, storage and configuration.
2. A standalone stdio process calling a new management API: possible, but adds another process and API contract for the same operations. A client-specific stdio bridge can be documented separately if required.
3. A standalone stdio process accessing persistence directly: would duplicate host composition and introduce a second library writer; not selected.

Personal tokens provide explicit credential configuration for clients that support authorization headers. They are not an OAuth authorization flow. Documentation must describe this distinction and avoid claiming compatibility with clients that require OAuth. Verify header support in each documented client before publishing its configuration example.

## First release tools

| Tool | Inputs and outcome | Required scope |
| --- | --- | --- |
| `search_audiobooks` | Optional title/author/series search, category and library filters, offset and limit; returns ordered summaries with stable IDs and pagination information. | `library:read` |
| `get_audiobook` | Audiobook ID; returns descriptive metadata and chapter summaries. | `library:read` |
| `get_metadata_suggestions` | Title, optional author, bounded result count; returns existing provider suggestions without changing a book. | `library:read` |
| `update_audiobook_metadata` | Audiobook ID and explicit field changes; returns the persisted descriptive metadata. | `library:write` |

Allow metadata updates to title, author, description, series, position in series, categories, release date, cover image URL and rating. Omitted fields remain unchanged. Explicit clear operations are permitted only for optional fields. Reject blank required text, invalid categories, invalid dates, negative series positions and ratings outside 0–100. Image URLs are stored as metadata; this tool does not fetch arbitrary URLs.

Book IDs, library IDs, chapter files, processing state, duration and archives are not writable metadata fields. The tool accepts a purpose-built input rather than a complete `AudioBook` entity.

Deletion, uploads, changes to sharing, playback actions, bulk processing and bulk metadata updates are outside this first release. These capabilities need their own operations and authorization rules.

## Personal token lifecycle

Add a token management component to the existing user profile. Require a signed-in cookie-authenticated user to access it. Provide token creation, listing and revocation through intention-named application operations.

When creating a token, require a name and expiry, and offer read-only or read-and-write access. Default to read-only and 90 days; enforce an expiry no more than 365 days from issuance. Write access includes read access. Token scope never overrides the owner's library permissions or grants administrator operations.

Generate an opaque token with a format/version prefix, a nonsecret random lookup ID, and at least 32 cryptographically random secret bytes. Display the complete credential only in the creation result. Store a SHA-256 digest of the secret and compare digests in constant time. High-entropy generated secrets do not use the account password.

Persist a product-specific Marten token document containing the lookup ID, owner ID, display name, secret digest, scopes, creation time, expiry and revocation time. List results contain only nonsecret metadata. Token retrieval for authentication uses a focused internal persistence contract that works before user authentication; token management specifications always restrict records to the current owner.

Persist revocation before reporting success. Check expiry, revocation and current owner state on every MCP HTTP request without a positive authentication cache. Requests authenticated before a revocation may finish; subsequent requests must fail. Load current user claims for each request so removed sharing access does not remain embedded in a token.

## Authentication and authorization

Register a dedicated personal-token authentication scheme and require it for `/mcp`. Accept credentials exclusively through `Authorization: Bearer <token>`. Cookie login alone does not authorize MCP. Token creation and revocation require cookie authentication and cannot be performed using a personal token.

Use HTTPS for deployed endpoints. Explicit development configuration may permit HTTP on loopback only. Configure accepted host names for the actual deployment; do not enable broad CORS for desktop clients. Redact authorization headers and credentials from application and request logging.

Return HTTP 401 for missing, malformed, unknown, expired or revoked credentials, or an unavailable owner account. Use a generic error that does not reveal account or token existence. Return HTTP 403 for an authenticated caller lacking a required endpoint permission; tool failures use the SDK's structured error response with an access-denied code.

Every tool must enforce its scope, including calls made directly through `tools/call`. Tool listings can reflect granted scopes, but hiding a tool is not enforcement. Use stateless HTTP transport so an earlier MCP session cannot retain authorization after revocation.

Reads are restricted to the owner's current `AvailableLibraries`. Updates in the first release are restricted to the owner's own library (`UsersLibraryName`); shared-library visibility does not grant MCP write access. Return the same not-found result for nonexistent and inaccessible audiobook IDs. Administrator roles do not bypass these MCP restrictions.

Attach an immutable server-created capability context to the authenticated request. Keep it available during synchronous application dispatch; do not trust user IDs, library permissions or scopes supplied as tool arguments. Verify its interaction with `UserIdSetterWolverineMiddleware`, which currently reconstructs user claims for message execution. Scope and library checks must survive that boundary.

## Application and persistence boundaries

Create `DotCast.Library.Mcp` for SDK tool registration, DTO mapping, descriptions and transport-facing error conversion. Compose it in `DotCast.App` using the official MCP C# SDK's ASP.NET Core integration. Keep token authentication and token management in a focused product-specific project, `DotCast.Infrastructure.PersonalApiTokens`, composed by the application.

Follow existing `IMessagePublisher` and Wolverine dispatch rather than adding a second dispatcher. New writes use intention-named request/handler pairs; reads use specifications and tailored projections. UI code, authentication adapters and MCP tools do not write repositories directly. Keep one top-level type per file and user-visible UI text in resources.

Add a dedicated library search specification that applies access filtering, search predicates, deterministic ordering and paging in Marten before materialization. Default to 20 results and cap at 100. Search uses case-insensitive matching for title, author and series; accent-insensitive matching is not required for this release. Do not reuse the existing specification that materializes the entire library before filtering.

For metadata updates, load the current record in a transaction, verify current ownership, validate the supplied patch and change only allowed fields. Serialize competing metadata writes for that book through the database transaction/row lock; do not replace a detached client-supplied entity. Keep transaction handling behind a focused persistence contract. Publish a completed metadata-update notification only after persistence succeeds. Preserve request cancellation through queries, external suggestion providers and writes.

The existing `AudioBookEditedHandler` accepts a full entity without an ownership check. It must not be called directly by MCP. This release introduces the narrow metadata operation without unrelated rewriting of the current UI edit workflow.

## Errors, logging and limits

Define structured outcomes for not found, access denied, invalid input and temporary provider failure. Unexpected errors are logged with a correlation ID and returned without stack traces or persistence details.

Write structured audit logs for token creation/revocation and metadata changes, identifying owner ID, nonsecret token ID, audiobook ID, changed field names and outcome. Do not log credentials, hashes, full descriptions or request bodies. Log success only after the corresponding commit. Use the existing application log destination.

Apply an MCP rate limit per authenticated token (60 requests per minute, no queued requests) and an unauthenticated limit per source IP (20 per minute). Return HTTP 429 when exceeded. Bound search text to 200 characters, provider suggestions to 10 items and JSON request bodies to 64 KiB. Trust forwarded IP/HTTPS information only from explicitly configured proxies.

## Verification and delivery

Meaningful automated checks must cover:

- Token creation returns the credential once, stores only its digest, validates expiry and enforces ownership for listing/revocation.
- Missing, malformed, expired and revoked tokens fail; cookie-only requests fail; removed users and sharing grants cannot retain access.
- Read-only tokens cannot update metadata, including direct tool invocations and dispatch through Wolverine.
- Search/details exclude inaccessible libraries; writes reject shared and foreign libraries and prohibited fields.
- Metadata patches preserve omitted fields and storage information; competing writes do not silently lose fields.
- A real HTTP MCP client can discover tools, search, retrieve details, update with write permission and observe revocation on the next HTTP request.
- Rate limits and size limits produce useful responses; logs do not contain test credentials.

Build affected projects and run relevant tests. Inspect the profile component in a running application when its dependencies are available. Add verified client configuration instructions using the actual DotCast URL and an environment/secret-store credential reference; never commit a working personal token. Verify any required Claude Desktop bridge before documenting it as supported.

## Acceptance

A signed-in user can issue and revoke a personal token in their profile. A supported AI client can authenticate to DotCast, inspect accessible audiobooks and maintain metadata in the user's own library according to token scope. Revocation and changed account/library access take effect on subsequent requests. Existing web cookie login continues to work.

The user must review this specification before implementation planning, as required by the repository's brainstorming skill.
