# Bitbucket MCP Server

A Model Context Protocol (MCP) server that provides a wrapper for Bitbucket Cloud REST API, enabling Pull Request operations through AI-powered tools. Built with Microsoft Kiota for type-safe API client generation.

## Features

- **MCP Protocol Support**: Full implementation of Model Context Protocol over **HTTP/SSE transport** (Streamable HTTP)
- **Pull Request Management**: 
  - Create pull requests with customizable settings
  - Update PR title, description, and reviewers
  - Toggle between open and draft status
  - Retrieve PR details
  - **NEW**: List pull requests with state filtering
- **Kiota-Generated Client**: Type-safe API client auto-generated from Bitbucket OpenAPI specification
- **Flexible Authentication**: Support for both Bitbucket app passwords and OAuth 2.0 tokens
- **Docker Support**: Ready-to-use Docker container with multi-stage build
- **Modern .NET**: Built on .NET 10 with ASP.NET Core for HTTP transport

## Prerequisites

- Docker (for containerized deployment) OR
- .NET 10 SDK (for local development)
- Bitbucket Cloud account
- Bitbucket app password or OAuth token
- **For development**: Microsoft Kiota CLI tool (installed locally via dotnet tool)

## Quick Start

### Using Docker

1. **Clone the repository**:
   ```bash
   git clone https://github.com/kubis1982/BitbucketMCP.git
   cd BitbucketMCP
   ```

2. **Configure authentication** in `docker-compose.yml`:
   
   For app password authentication:
   ```yaml
   environment:
     ASPNETCORE_URLS: "http://+:8080"
     BITBUCKET_AUTH_TYPE: "app_password"
     BITBUCKET_USERNAME: "your-username"
     BITBUCKET_APP_PASSWORD: "your-app-password"
   ```
   
   For OAuth token authentication:
   ```yaml
   environment:
     ASPNETCORE_URLS: "http://+:8080"
     BITBUCKET_AUTH_TYPE: "oauth_token"
     BITBUCKET_TOKEN: "your-oauth-token"
   ```

3. **Run the server**:
   ```bash
   docker-compose up -d
   ```
   
   The server will be available at: **http://localhost:8080**

4. **Test the connection**:
   ```bash
   # Test with MCP-compatible client (requires Accept: text/event-stream header)
   curl -H "Accept: text/event-stream" http://localhost:8080/
   ```

### Local Development

1. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

2. **Set environment variables**:
   ```bash
   # Windows (PowerShell)
   $env:ASPNETCORE_URLS="http://localhost:5000"
   $env:BITBUCKET_AUTH_TYPE="app_password"
   $env:BITBUCKET_USERNAME="your-username"
   $env:BITBUCKET_APP_PASSWORD="your-app-password"
   
   # Linux/Mac
   export ASPNETCORE_URLS=http://localhost:5000
   export BITBUCKET_AUTH_TYPE=app_password
   export BITBUCKET_USERNAME=your-username
   export BITBUCKET_APP_PASSWORD=your-app-password
   ```

3. **Run the server**:
   ```bash
   dotnet run --project src/BitbucketMCP
   ```
   
   The server will be available at: **http://localhost:5000**

## Configuration

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `ASPNETCORE_URLS` | No | Server listening URL (default: http://localhost:5000, Docker: http://+:8080) |
| `BITBUCKET_AUTH_TYPE` | Yes | Authentication method: `app_password` or `oauth_token` |
| `BITBUCKET_USERNAME` | Conditional | Required for `app_password` auth |
| `BITBUCKET_APP_PASSWORD` | Conditional | Required for `app_password` auth |
| `BITBUCKET_TOKEN` | Conditional | Required for `oauth_token` auth |
| `BITBUCKET_WORKSPACE` | No | Default workspace for operations |
| `ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT` | No | Logging level (Trace, Debug, Information, Warning, Error) |

### MCP Connection

The server uses **HTTP/SSE (Server-Sent Events)** transport, implementing the Model Context Protocol's Streamable HTTP specification.

**Connection Details**:
- **Local Development**: `http://localhost:5000`
- **Docker Deployment**: `http://localhost:8080`
- **Protocol**: MCP over HTTP with Server-Sent Events
- **Required Headers**: `Accept: text/event-stream`

**Example MCP Client Configuration** (Claude Desktop):
```json
{
  "mcpServers": {
    "bitbucket": {
      "url": "http://localhost:8080/",
      "transport": {
        "type": "http"
      }
    }
  }
}
```

**Testing the endpoint**:
```bash
# The server expects MCP protocol headers
curl -H "Accept: text/event-stream" http://localhost:8080/

# For full MCP communication, use an MCP-compatible client
```

### Authentication Setup

#### App Password (Recommended for Personal Use)

1. Go to Bitbucket Settings → Personal settings → App passwords
2. Click "Create app password"
3. Grant required permissions:
   - **Pull requests**: Read, Write
   - **Repositories**: Read
4. Copy the generated password (shown only once)
5. Use your Bitbucket username and the app password

#### OAuth 2.0 Token (Recommended for Production)

1. Create an OAuth consumer in your workspace settings
2. Configure appropriate scopes
3. Obtain an access token through OAuth flow
4. Use the bearer token for authentication

## MCP Tools

The server exposes the following MCP tools:

### 1. CreatePullRequest

Creates a new pull request in a Bitbucket repository.

**Parameters**:
- `workspace` (required): Workspace slug
- `repo` (required): Repository slug
- `title` (required): PR title
- `description` (optional): PR description
- `sourceBranch` (required): Source branch name
- `destinationBranch` (required): Destination branch name
- `reviewers` (optional): Array of reviewer account UUIDs in format `{account-id}`
- `isDraft` (optional): Create as draft PR (default: false)

**Example**:
```json
{
  "workspace": "myworkspace",
  "repo": "myrepo",
  "title": "Add new feature",
  "description": "This PR adds amazing new features",
  "sourceBranch": "feature/new-feature",
  "destinationBranch": "main",
  "reviewers": ["{user-uuid-1}", "{user-uuid-2}"],
  "isDraft": false
}
```

### 2. UpdatePullRequest

Updates an existing pull request.

**Parameters**:
- `workspace` (required): Workspace slug
- `repo` (required): Repository slug
- `prId` (required): PR ID number
- `title` (optional): New PR title
- `description` (optional): New PR description
- `reviewers` (optional): Updated array of reviewer account UUIDs
- `isDraft` (optional): Change draft status

**Example**:
```json
{
  "workspace": "myworkspace",
  "repo": "myrepo",
  "prId": 123,
  "title": "Updated title",
  "description": "Updated description"
}
```

### 3. GetPullRequest

Retrieves details of a pull request.

**Parameters**:
- `workspace` (required): Workspace slug
- `repo` (required): Repository slug
- `prId` (required): PR ID number

**Example**:
```json
{
  "workspace": "myworkspace",
  "repo": "myrepo",
  "prId": 123
}
```

### 4. ListPullRequests

Lists pull requests in a repository with optional state filtering.

**Parameters**:
- `workspace` (required): Workspace slug
- `repo` (required): Repository slug
- `state` (optional): Filter by state - `OPEN`, `MERGED`, `DECLINED`, `SUPERSEDED` (if not specified, defaults to `OPEN`)

**Example**:
```json
{
  "workspace": "myworkspace",
  "repo": "myrepo",
  "state": "OPEN"
}
```

## API Mapping

The server wraps the following Bitbucket Cloud REST API v2.0 endpoints:

| MCP Tool | HTTP Method | Bitbucket API Endpoint |
|----------|-------------|------------------------|
| CreatePullRequest | POST | `/repositories/{workspace}/{repo}/pullrequests` |
| UpdatePullRequest | PUT | `/repositories/{workspace}/{repo}/pullrequests/{pr_id}` |
| GetPullRequest | GET | `/repositories/{workspace}/{repo}/pullrequests/{pr_id}` |
| **ListPullRequests** | **GET** | **`/repositories/{workspace}/{repo}/pullrequests`** |

Base URL: `https://api.bitbucket.org/2.0`

## Building from Source

```bash
# Clone the repository
git clone https://github.com/kubis1982/BitbucketMCP.git
cd BitbucketMCP

# Build the project
dotnet build

# Run tests (if available)
dotnet test

# Publish for deployment
dotnet publish -c Release -o ./publish
```

## Docker Build

```bash
# Build the Docker image
docker build -t bitbucket-mcp:latest .

# Run the container with port mapping for HTTP/SSE
docker run -d --rm \
  -p 8080:8080 \
  -e ASPNETCORE_URLS="http://+:8080" \
  -e BITBUCKET_AUTH_TYPE=app_password \
  -e BITBUCKET_USERNAME=your-username \
  -e BITBUCKET_APP_PASSWORD=your-password \
  bitbucket-mcp:latest
```

## Troubleshooting

### Authentication Errors

**Problem**: `401 Unauthorized` errors

**Solution**:
- Verify your credentials are correct
- For app passwords, ensure you're using your Bitbucket username (not email)
- Check that app password has required permissions
- For OAuth, verify token hasn't expired

### Connection Issues

**Problem**: Cannot connect to Bitbucket API

**Solution**:
- Check internet connectivity
- Verify firewall settings allow HTTPS outbound connections
- Ensure `https://api.bitbucket.org` is accessible

### MCP Protocol Issues

**Problem**: MCP client cannot communicate with server

**Solution**:
- Verify server is running and listening on correct port (5000 local, 8080 Docker)
- Ensure MCP client is configured with correct HTTP endpoint URL
- Check that client is sending `Accept: text/event-stream` header
- Verify firewall allows connections on the server port
- Check server logs for error messages (use `docker logs` or console output)
- Validate JSON-RPC message format in MCP protocol communication

## Development

### Project Structure

```
BitbucketMCP/
├── .config/
│   └── dotnet-tools.json       # Local .NET tools manifest (Kiota)
├── openapi/
│   └── bitbucket-swagger.json  # Bitbucket OpenAPI specification
├── scripts/
│   ├── generate-client.ps1     # PowerShell script for client generation
│   └── generate-client.bat     # Batch wrapper for generation script
├── src/
│   └── BitbucketMCP/
│       ├── Configuration/      # Configuration models
│       ├── Generated/          # Kiota-generated API client (auto-generated)
│       ├── Tools/              # MCP tool implementations
│       └── Program.cs          # Application entry point + DI + inline auth providers
├── docs/                       # Documentation
├── Dockerfile                  # Docker configuration
├── docker-compose.yml          # Docker Compose setup
└── README.md                   # This file
```

**Note**: The `Services/` and `Models/` folders have been removed. Tools now use the Kiota-generated client and models directly, eliminating the abstraction layer.

### Regenerating API Client

The Bitbucket API client is generated from the official OpenAPI specification using Microsoft Kiota. To regenerate after API updates:

1. **Restore tools**:
   ```bash
   dotnet tool restore
   ```

2. **Run generation script**:
   ```bash
   # Windows
   .\scripts\generate-client.bat
   
   # Linux/Mac with PowerShell
   pwsh ./scripts/generate-client.ps1
   ```

3. **Verify changes**:
   ```bash
   dotnet build
   ```

The script will:
- Download the latest OpenAPI spec from Bitbucket
- Clean the `src/BitbucketMCP/Generated/` directory
- Generate a fresh API client with all models and request builders
- Output generation summary

### Adding New Tools

1. Create a new class in `Tools/` directory
2. Add `[McpServerToolType]` attribute to the class
3. Create methods with `[McpServerTool]` attribute
4. Add parameter descriptions with `[Description]` attribute
5. Use the Kiota-generated client directly (injected via constructor)

Example:
```csharp
using BitbucketMCP.Generated;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public class MyCustomTool(BitbucketApiClient client)
{
    [McpServerTool(Name = "my_custom_tool")]
    [Description("Does something useful with pull requests")]
    public async Task<string> DoSomething(
        [Description("The workspace slug")] string workspace,
        [Description("The repository slug")] string repo)
    {
        var prs = await client.Repositories[workspace][repo].Pullrequests.GetAsync();
        return $"Found {prs?.Values?.Count ?? 0} pull requests";
    }
}
```

### Architecture Notes

- **HTTP/SSE Transport**: Server uses ASP.NET Core with Streamable HTTP transport (MCP SDK 1.2.0+)
- **Direct Kiota Usage**: Tools use the `Generated/BitbucketApiClient` directly without any wrapper layer
- **No Model Mapping**: Tools work with Kiota-generated models (`Pullrequest`, `Account`, etc.) and format output as needed
- **Inline Authentication**: Custom authentication providers are defined as file-scoped classes in `Program.cs`
- **Simplified DI**: `Program.cs` registers only the Kiota client and authentication; Tools receive the client via constructor injection
- **Type Safety**: Full IntelliSense and compile-time checking for all API operations
- **Docker Ready**: Uses `mcr.microsoft.com/dotnet/aspnet:10.0` runtime image with port 8080 exposed

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- API client generated with [Microsoft Kiota](https://github.com/microsoft/kiota)
- Powered by [Bitbucket Cloud REST API v2.0](https://developer.atlassian.com/cloud/bitbucket/rest/)
- Uses [.NET 10](https://dot.net)

## Support

For issues, questions, or contributions, please visit:
- GitHub Issues: https://github.com/kubis1982/BitbucketMCP/issues
- Bitbucket API Documentation: https://developer.atlassian.com/cloud/bitbucket/rest/

## Version

Current version: 1.0.0