# Library management through MCP

DotCast exposes a stateless Streamable HTTP MCP endpoint at `/mcp`, authenticated with a personal API key.

## API key

Sign in and open **Account → API key**. Click **Generate key** and save the value shown once. Each account has one key. **Regenerate key** immediately replaces the previous key; **Revoke key** removes access. Keys have no scopes and no expiry.

The key allows all four MCP tools under the account's normal library access. It can read shared libraries and update metadata in the account's own library. It cannot manage other accounts or change sharing. The server stores only a SHA-256 digest of the random key. Removed sharing, account lockout/deletion, regeneration and revocation affect subsequent requests; an already authenticated request may finish.

Account API keys are owned by `DotCast.Infrastructure.ApiKeys`, independently of MCP. It supplies key storage, management handlers and the authentication scheme; MCP consumes that scheme. Key management runs through the existing authenticated Blazor profile and application handlers. There is no separate token-management HTTP API.

Metadata updates use the existing infrastructure repository's `GetAndUpdateAsync` operation. The handler checks library ownership before applying the patch, and the repository handles the transaction and concurrency retry. API key generation uses an atomic repository upsert.

## Connect Codex

Use the actual DotCast HTTPS address followed by `/mcp`. For the repository's Aspire development profile:

```powershell
codex mcp add dotcast --url https://localhost:7062/mcp --bearer-token-env-var DOTCAST_API_KEY
```

Provide `DOTCAST_API_KEY` to the process that starts Codex through your environment or secret manager. Restart a desktop app after changing its inherited environment. Keep the value out of source control and shared configuration. The equivalent TOML entry is:

```toml
[mcp_servers.dotcast]
url = "https://localhost:7062/mcp"
bearer_token_env_var = "DOTCAST_API_KEY"
```

See the [official Codex MCP documentation](https://developers.openai.com/codex/mcp/). Trust the development certificate normally; do not disable certificate verification. This is an API-key flow; `codex mcp login` is for OAuth.

Other clients need Streamable HTTP support and `Authorization: Bearer <key>`. A Claude Desktop stdio bridge is not included or verified.

## Tools

| Tool | Behavior |
| --- | --- |
| `search_audiobooks` | Search accessible books by title, author or series, with category/library filters and pagination. Default 20, maximum 100 results. |
| `get_audiobook` | Get metadata, chapter summaries and allowed category names. |
| `get_metadata_suggestions` | Retrieve suggestions from configured providers without saving changes. Default 5, maximum 10. |
| `update_audiobook_metadata` | Patch selected metadata fields in the account's own library. |

Metadata providers run concurrently with up to three detail requests per source, while preserving source preference and search order. Each HTTP request has a five-second timeout and each provider a ten-second search deadline. A failed or timed-out source keeps any suggestions already found and allows the other source to respond. If every source fails without suggestions, the lookup reports an error.

Update arguments:

```json
{"id":"a-book-id","changes":{"title":"Corrected title","author":"Author name","rating":85}}
```

Allowed fields: `title`, `author`, `description`, `series`, `positionInSeries`, `releaseDate`, `imageUrl`, `categories`, `rating`. Omitted fields retain their values. `null` clears description, series, release date or image URL; an empty array clears categories. Dates use `yyyy-MM-dd`, ratings use 0–100, series positions cannot be negative, and title/author must be nonblank with at most 500 characters. Image URLs must use HTTP/S and are stored without downloading their content.

IDs, libraries, chapter files, deletion, uploads and sharing changes are not exposed.

## Hosting

Set `Mcp:AllowedHosts` (or `Mcp__AllowedHosts`) to accepted host names separated by semicolons. Default: `localhost;127.0.0.1;[::1]`. Serve HTTPS. For TLS termination, configure forwarded headers with explicitly trusted proxies, or use TLS to DotCast. Keep Authorization headers and request bodies out of proxy logs.

`Mcp:AllowLoopbackHttp=true` is only effective in Development when both the connecting IP and host are loopback. Desktop clients do not require CORS.

Requests are limited to 64 KiB, including chunked bodies. Limits are 60 requests/minute/key and 20/minute/IP for unauthenticated traffic, per application instance. Search/filter text is bounded to 200 characters. Invalid keys return 401 and rate limits return 429. Tool errors have a sanitized code, message and correlation ID; missing and inaccessible books both return `not_found`.

## Verification

```powershell
dotnet build DotCast.App/DotCast.App.csproj
dotnet test DotCast.Infrastructure.ApiKeys.Tests/DotCast.Infrastructure.ApiKeys.Tests.csproj
dotnet test DotCast.Library.Mcp.Tests/DotCast.Library.Mcp.Tests.csproj
```

Tests use Docker with disposable PostgreSQL containers and a real HTTP MCP client. They cover key replacement/revocation, account boundaries, all four tools, request limits and concurrent metadata changes.
Provider tests also cover concurrent requests, result ordering and limits, cancellation, timeout/failure fallback and both Goodreads search layouts.
The API key suite runs without MCP and verifies credentials, authentication on an ordinary HTTP endpoint and regeneration/revocation.
