# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

There is no `.sln` file — operate on the two projects under `src/` directly, or let `dotnet` discover them from the repo root.

```bash
# Restore
dotnet restore

# Build
dotnet build

# Run all tests
dotnet test

# Run a single test (xUnit v3, by fully qualified name)
dotnet test --filter "FullyQualifiedName~PullResponseTests.MethodName"

# Run the server locally (stdio is the default transport)
dotnet run --project src/BitbucketMCP -- --transport=stdio
dotnet run --project src/BitbucketMCP -- --transport=http

# Pack the NuGet tool package (embeds src/BitbucketMCP/.mcp/server.json)
dotnet pack src/BitbucketMCP/BitbucketMCP.csproj --configuration Release

# Docker builds (context must be repo root, not src/)
docker build -f stdio/Dockerfile -t bitbucket-mcp:latest .
docker build -f http/Dockerfile -t bitbucket-mcp:http-latest .
```

Required environment variables when running the server: `BITBUCKET_USERNAME`, `BITBUCKET_APP_PASSWORD`, `BITBUCKET_WORKSPACE`. `ASPNETCORE_URLS` only matters for the HTTP transport.

## Architecture

This is a Model Context Protocol (MCP) server that exposes Bitbucket Cloud pull-request operations as MCP tools. It's a thin wrapper: MCP tool → generated Kiota client → Bitbucket REST API v2.0.

- **`Program.cs`** — top-level entry point. Parses `--transport=stdio|http` (default `stdio`), validates `BitbucketConfig` from env vars, then builds either a `Host` (stdio, via `AddMcpServer().WithStdioServerTransport()`) or a `WebApplication` (HTTP, via `AddMcpServer().WithHttpTransport()` + `app.MapMcp()`). Both paths call `WithToolsFromAssembly()`, which auto-discovers every `[McpServerToolType]` class.
- **`McpServiceCollectionExtensions.RegisterServices`** — the only DI wiring in the app. Registers `BitbucketConfig`, a Kiota `IAuthenticationProvider` (`BasicAuthProvider`, a file-scoped class in the same file that Base64-encodes username/app-password into a Basic `Authorization` header), an `HttpClient` named `"Kiota"` pointed at `https://api.bitbucket.org/2.0`, an `HttpClientRequestAdapter`, and the generated `BitbucketRestClient`.
- **`Tools/*.cs`** — one class per MCP tool (`CreatePullRequestTool`, `UpdatePullRequestTool`, `GetPullRequestTool`, `ListPullRequestsTool`), each `[McpServerToolType]` with a single `[McpServerTool(Name = "...")]` method. Tools receive `BitbucketRestClient` and `BitbucketConfig` via constructor injection and call the fluent Kiota client (e.g. `client.Repositories[workspace][repo].Pullrequests.PostAsync(...)`). Parameters use `[Description(...)]` attributes — these strings are surfaced to MCP clients/LLMs as the tool's parameter docs, so keep them accurate when changing signatures.
- **`Models/PullResponse.cs`** — a hand-shaped DTO (`PullResponse.From(Pullrequest)`) that flattens the generated Kiota `Pullrequest` model into what tools actually return, decoupling the MCP-facing shape from the generated API client's shape.
- **`Kubis1982.Atlassian.Bitbucket.RestClient`** (the `Kubis1982.Atlassian.Bitbucket.RestClient.v2` NuGet package) is the Kiota-generated Bitbucket API client; its namespaces are pulled in globally via `GlobalUsings.cs`. Do not hand-edit generated client code — it isn't part of this repo.
- **`src/BitbucketMCP/.mcp/server.json`** — the MCP registry manifest (per `server.schema.json`), packed into the NuGet package via `McpServerJsonTemplateFile`/`Pack="true"` in `BitbucketMCP.csproj`. Its `version` field is not auto-synced with `Directory.Build.props` — bump it manually on release if it needs to match.

## Versioning and releases

- The single source of truth for the package version is `<Version>` in `Directory.Build.props` (currently `3.2.1`).
- Releases are tag-driven (`.github/workflows/release.yml`, triggered on `v*.*.*` tags): it validates the tag matches `Directory.Build.props`, runs tests, then in parallel publishes the NuGet package, builds/pushes both Docker image variants (stdio and HTTP, to Docker Hub and GHCR), and creates a GitHub Release with an auto-generated changelog from Conventional Commits (`feat`, `fix`, `docs`, `perf`, `refactor`, etc.).
- Bump `Directory.Build.props` and commit to `main` *before* creating the release tag, or the `validate-version` job fails the release.

## Transport images

Two independent Dockerfiles build from the same source with different entry commands: `stdio/Dockerfile` (default, tagged `latest`, no exposed ports, for process-based MCP clients like Claude Desktop) and `http/Dockerfile` (tagged `http-latest`, exposes 8080, Streamable HTTP/SSE transport). Both must be built with the repo root as Docker context (they need `Directory.Build.props`).
