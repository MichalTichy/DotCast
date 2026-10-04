# Library management through MCP

DotCast hosts a stateless Streamable HTTP MCP endpoint at `/mcp`. It uses personal API tokens tied to DotCast accounts. A web login cookie does not authenticate an MCP client.

## Create a token

Sign in to DotCast and open **Account → Personal API tokens**. Enter a name and expiry and choose read-only access or allow metadata changes. Save the credential shown after creation; its value is displayed only at issuance. The server stores a digest rather than the credential.

Default expiry is 90 days, with a maximum of 365 days. Revoke tokens from the same account page. Revocation, account lockout/deletion and removed sharing access affect subsequent requests. Requests already authenticated may finish.

Read access includes your currently accessible shared libraries. Write access changes metadata in your own library only. Token scope does not grant administrator privileges or write access to another user's library.

## Server configuration

Use your actual DotCast HTTPS origin followed by `/mcp`. Set accepted host names explicitly in the application's configuration:

```json
{
  "Mcp": {
    "AllowedHosts": "dotcast.example.com;localhost;127.0.0.1;[::1]",
    "AllowLoopbackHttp": false
  }
}
```

Replace `dotcast.example.com` with the deployment's hostname. The default accepted hosts are loopback names. `Mcp__AllowedHosts` is the equivalent environment variable; Azure App Configuration keys follow the application's existing `dotcast:` prefix convention. Wildcards are not accepted as host-name matches.

Serve the endpoint over HTTPS. If TLS terminates at a proxy, configure ASP.NET Core forwarded-header handling with explicitly trusted proxy addresses, or use TLS from the proxy to DotCast. Desktop clients do not need CORS. Keep Authorization headers and request bodies out of proxy logs.

For local development only, `Mcp:AllowLoopbackHttp=true` permits HTTP when the host and connecting IP are both loopback and the application's environment is Development. Aspire's existing HTTPS application URL can be used without that setting.

## Connect Codex

The local CLI's `codex mcp add --help` and [official OpenAI documentation](https://developers.openai.com/codex/mcp/) support reading Bearer credentials from an environment variable. Configure the URL using the actual running DotCast address. For the repository's Aspire development profile:

```powershell
codex mcp add dotcast --url https://localhost:7062/mcp --bearer-token-env-var DOTCAST_API_TOKEN
```

Provide `DOTCAST_API_TOKEN` to the process that starts Codex from your environment or a secret manager. A desktop app must be restarted after changing its inherited environment. Keep the credential out of source control and shared configuration. The equivalent TOML entry is:

```toml
[mcp_servers.dotcast]
url = "https://localhost:7062/mcp"
bearer_token_env_var = "DOTCAST_API_TOKEN"
```

For another deployment, replace that URL. Trust your local development certificate normally; do not disable certificate verification. `codex mcp login` is an OAuth flow and is not used for these personal tokens.

Other MCP clients need Streamable HTTP support and the ability to send `Authorization: Bearer <credential>`. Clients that require OAuth cannot use this endpoint's personal-token flow directly. A Claude Desktop stdio bridge is not included or verified in this release.

## Tools

| Tool | Behavior |
| --- | --- |
| `search_audiobooks` | Searches title, author and series without case sensitivity; supports category/library filters and pagination. Default 20, maximum 100 results. |
| `get_audiobook` | Returns metadata, chapter summaries and allowed category names for an accessible book ID. |
| `get_metadata_suggestions` | Looks up metadata from the configured providers. Default 5, maximum 10 suggestions. Does not modify books. |
| `update_audiobook_metadata` | Applies selected changes to a book in the account's own library; requires write access. |

Example arguments for metadata maintenance:

```json
{
  "id": "a-book-id-returned-by-search",
  "changes": {
    "title": "Corrected title",
    "author": "Author name",
    "series": "Series name",
    "positionInSeries": 2,
    "releaseDate": "2026-10-03",
    "categories": ["Science Fiction"],
    "rating": 85
  }
}
```

Allowed fields are `title`, `author`, `description`, `series`, `positionInSeries`, `releaseDate`, `imageUrl`, `categories` and `rating`. Omitted fields retain their values. `null` clears description, series, release date or image URL; an empty category array clears categories. Dates use `yyyy-MM-dd`, ratings use 0–100, and series positions cannot be negative. Required title/author text is nonblank and at most 500 characters. Cover image URLs use HTTP or HTTPS and are stored without downloading their content.

Library IDs and chapter/storage information cannot be changed. Deletion, upload, sharing changes and bulk processing are not exposed by these tools.

## Errors and limits

Invalid credentials return HTTP 401 with a Bearer challenge. Requests exceeding the rate limit return 429. Maximum JSON request size is 64 KiB, including chunked uploads. Limits are 60 requests per minute per token and 20 per minute per source IP for unauthenticated traffic. Limits are per application instance; use a shared proxy limiter when deploying multiple replicas. Search text and filters are bounded to 200 characters.

Tool errors have `isError=true` and structured content containing `error.code` and a correlation ID. Codes include `access_denied`, `not_found`, `invalid_input`, `provider_unavailable` and `internal_error`. Missing and inaccessible book IDs both return `not_found`. Audit logs identify the account, token ID, book ID and changed field names without the credential or metadata values.

## Verification

```powershell
dotnet build DotCast.App/DotCast.App.csproj
dotnet test DotCast.Library.Mcp.Tests/DotCast.Library.Mcp.Tests.csproj
```

Integration tests start and clean up a disposable PostgreSQL container through Testcontainers, so Docker must be running. They exercise a real HTTP MCP client, Wolverine's user-claim reconstruction, Marten filtering and serialized metadata updates, cookie/CSRF token management, token revocation and account access changes.
