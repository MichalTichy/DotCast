# Library MCP Implementation Plan

> For agentic workers: execute the approved specification task by task in this session, with focused automated verification after each slice.

**Goal:** Give AI clients authenticated search and metadata maintenance through personal tokens.
**Architecture:** Host stateless HTTP MCP in DotCast.App. Keep tokens in their own product-specific infrastructure project and expose narrow library operations through Wolverine. Carry token capabilities in a trusted HTTP request feature independently of reconstructed user claims.
**Tech Stack:** .NET 10, ASP.NET Core, MCP C# SDK, Marten, Wolverine, Blazor, xUnit.
**Spec:** ../specs/2026-10-03-library-mcp-design.md

## Global constraints

- `/mcp` requires personal Bearer authentication; token management requires cookie authentication.
- Tokens use a 32-byte random secret, SHA-256 digest, owner, scopes and expiry. Default expiry is 90 days; maximum is 365 days.
- Read scope covers current accessible libraries; write scope covers the owner's own library only.
- Search defaults to 20 and caps at 100; suggestion results cap at 10; text caps at 200 characters; requests cap at 64 KiB.
- Each handwritten top-level type gets its own file. Use existing dispatch and repositories/specifications.

## Review focus

- Malformed credential inputs must fail without leaking secret or parser details.
- Reconstructed claims and per-message DI scopes must not lose token restrictions.
- Explicit null and omitted metadata changes must have distinct behavior.
- Revocation and sharing removal must work without restarting an MCP connection.
- Unsupported clients must not receive unverified credential configuration instructions.

## Task 1: Tokens and authentication

**Files:** create `DotCast.Infrastructure.PersonalApiTokens/` with Models, Authentication, Persistence, UseCases and Texts; add focused tests in `DotCast.Library.Mcp.Tests/`.
**Interfaces:** `IPersonalApiTokenStore` exposes credential lookup, owner list, add and revoke with cancellation; `IPersonalTokenContext.RequireScope(string)` returns an immutable identity; `ITokenOwnerResolver.FindAsync(string, CancellationToken)` resolves a current unlocked owner.

- [x] Add tests for malformed tokens, digest verification, expiry, revocation and owner restrictions.
- [x] Implement credential generation/parsing and scoped token documents/specifications using existing Marten infrastructure.
- [x] Implement create/list/revoke handlers and register them explicitly for Wolverine discovery.
- [x] Implement a dedicated authentication scheme and request feature independent of the claims middleware.
- [x] Run the token tests and verify no raw credential enters persistent data or logs.

## Task 2: Library operations

**Files:** create `DotCast.Library/Mcp/` models, specifications, handlers and a focused Marten metadata writer.
**Interfaces:** search returns summaries and pagination; detail returns descriptive metadata/chapter summaries; metadata update accepts ID plus an explicit patch and returns persisted detail.

- [x] Add tests for scope enforcement, own/shared/foreign libraries, omitted and cleared fields, prohibited fields and invalid metadata.
- [x] Implement search specifications with database filtering, ordering, projection and paging.
- [x] Implement narrow metadata validation and transactional updates with database serialization for competing MCP writes.
- [x] Publish success notifications after persistence and log only changed field names/identifiers.
- [x] Run library behavior tests and, when PostgreSQL is available, verify the actual Marten query/update slice.

## Task 3: HTTP MCP host

**Files:** create `DotCast.Library.Mcp/` for tools, registration and HTTP boundary; modify `DotCast.App/Program.cs`, application project references and solution.
**Interfaces:** `AddLibraryMcp(IServiceCollection)` registers the server; `MapLibraryMcp(WebApplication)` maps the secured endpoint and limits.

- [x] Add real HTTP client tests for discovery, search, details and updates with authenticated requests.
- [x] Register four tools from the specification with scope checks and structured sanitized errors.
- [x] Require personal-token authentication, stateless transport, deployed HTTPS and configured host validation.
- [x] Apply per-token and failed-auth IP rate limiting plus a 64 KiB body limit.
- [x] Test cookie-only denial, direct write calls using a read token, next-request revocation, sharing removal and limits.

## Task 4: Profile and delivery

**Files:** create `DotCast.App/Components/PersonalApiTokens/` markup/code-behind/resources; modify `DotCast.App/Pages/UserProfile.razor`; add `docs/library-mcp.md` and README link.

- [x] Add cookie-gated token creation/list/revoke to the profile using application operations and nonsecret list models.
- [x] Show credentials only in the creation response and allow dismissal.
- [x] Document HTTPS deployment and supported client credential configuration without committing credentials.
- [x] Build the application and run the focused test project plus existing relevant tests.
- [x] Inspect the running profile when dependencies are available; record any runtime verification limitation.
- [x] Review final diff for secret handling, authorization gaps, ownership and scope continuity, and scope/spec coverage.

## Verification results

- MCP tests: 29 passed against disposable PostgreSQL containers, including real HTTP discovery, all four tools, access removal, cookie/CSRF protection, request limits and concurrent metadata patches.
- Existing presigned URL tests: 3 passed.
- DotCast.App build: passed with existing dependency/compiler warnings.
- Profile smoke test: signed in with a disposable account, created a read token, hid the credential, revoked it and reloaded to confirm revoked status with no secret displayed.
- Claude Desktop bridge compatibility remains explicitly unverified in the client documentation.
