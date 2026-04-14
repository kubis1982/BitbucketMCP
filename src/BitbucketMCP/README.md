# Bitbucket MCP Server - NuGet Package

A Model Context Protocol (MCP) server package for Atlassian Bitbucket that provides AI-powered Pull Request management and code review assistance.

## Package Information

- **Package Type**: `McpServer` (installable via NuGet)
- **Package ID**: `Kubis1982.Atlassian.Bitbucket.MCP`
- **Tool Command**: `bitbucket-mcp`
- **Runtime**: .NET 10 (requires .NET runtime installed)
- **License**: MIT

### Supported Runtimes

- **dnx** (dotnet CLI)
- **dotnet** (when invoked via `dotnet tool`)

## Installation

### Option 1: As a Global Tool

Install the MCP server globally on your system:

```bash
dotnet tool install --global Kubis1982.Atlassian.Bitbucket.MCP
```

Once installed, you can invoke it with:
```bash
bitbucket-mcp
```

### Option 2: As a Local Tool

Install in a specific project:

```bash
# Create or update .config/dotnet-tools.json
dotnet tool install Kubis1982.Atlassian.Bitbucket.MCP

# Run with
dotnet bitbucket-mcp

```

### Option 3: Direct NuGet Installation

Add to your project file or install via Package Manager:

```bash
nuget install Kubis1982.Atlassian.Bitbucket.MCP
```

## Quick Start

### Prerequisites

- .NET 10 Runtime (or SDK)
- Bitbucket Cloud account
- Bitbucket App Password with PR and repository read permissions

### Configuration in MCP Clients

#### Claude Desktop Configuration

Add the following configuration to your Claude Desktop settings file:

**Location**:
- **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
- **Linux**: `~/.config/Claude/claude_desktop_config.json`

**Option 1: Using installed tool (Recommended if installed)**:

```json
{
  "mcpServers": {
    "bitbucket": {
      "command": "bitbucket-mcp",
      "args": ["--transport=stdio"],
      "env": {
        "BITBUCKET_USERNAME": "your-username",
        "BITBUCKET_APP_PASSWORD": "your-app-password",
        "BITBUCKET_WORKSPACE": "your-workspace"
      }
    }
  }
}
```

**Option 2: Using dnx (Direct NuGet execution - no installation required)**:

```json
{
  "mcpServers": {
    "bitbucket": {
      "command": "dnx",
      "args": ["Kubis1982.Atlassian.Bitbucket.MCP@3.1.0", "--yes --transport=stdio"],
      "env": {
        "BITBUCKET_USERNAME": "your-username",
        "BITBUCKET_APP_PASSWORD": "your-app-password",
        "BITBUCKET_WORKSPACE": "your-workspace"
      }
    }
  }
}
```

**Option 3: Using dotnet CLI**:

```json
{
  "mcpServers": {
    "bitbucket": {
      "command": "dotnet",
      "args": ["bitbucket-mcp", "--transport=stdio"],
      "env": {
        "BITBUCKET_USERNAME": "your-username",
        "BITBUCKET_APP_PASSWORD": "your-app-password",
        "BITBUCKET_WORKSPACE": "your-workspace"
      }
    }
  }
}
```

#### Other MCP Clients

For clients that support custom server definitions, use this configuration template:

```json
{
  "servers": {
    "bitbucket": {
      "type": "stdio",
      "command": "bitbucket-mcp",
      "args": ["--transport=stdio"],
      "env": {
        "BITBUCKET_USERNAME": "${env:BITBUCKET_USERNAME}",
        "BITBUCKET_APP_PASSWORD": "${env:BITBUCKET_APP_PASSWORD}",
        "BITBUCKET_WORKSPACE": "${env:BITBUCKET_WORKSPACE}"
      }
    }
  }
}
```

### Authentication Setup

1. Go to **Bitbucket Settings → Personal settings → App passwords**
2. Click **"Create app password"**
3. Enter an app password name (e.g., "MCP Server")
4. Grant required permissions:
   - ✅ **Pull requests**: Read, Write
   - ✅ **Repositories**: Read
5. Copy the generated password (shown only once)
6. Set environment variables:
   - `BITBUCKET_USERNAME`: Your Bitbucket username
   - `BITBUCKET_APP_PASSWORD`: The generated app password
   - `BITBUCKET_WORKSPACE`: Your Bitbucket workspace slug

## Configuration Options

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `BITBUCKET_USERNAME` | Yes | Bitbucket username for authentication |
| `BITBUCKET_APP_PASSWORD` | Yes | Bitbucket app password with PR permissions |
| `BITBUCKET_WORKSPACE` | Yes | Bitbucket workspace slug |
| `ASPNETCORE_URLS` | No* | Server URL (HTTP transport only, default: `http://localhost:5000`) |
| `ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT` | No | Logging level (Trace, Debug, Information, Warning, Error) |

*Only used when running with `--transport=http`

### Command Line Arguments

```bash
bitbucket-mcp [OPTIONS]
```

**Options**:
- `--transport=stdio` - Use standard input/output transport (default, recommended for MCP clients)
- `--transport=http` - Use HTTP/SSE transport for web-based clients
- `--help` - Display help information
- `--version` - Display version information

## Available Tools

The MCP server exposes the following tools for AI assistants:

### 1. CreatePullRequest

Creates a new pull request in a Bitbucket repository.

**Parameters**:
- `repo` (required): Repository slug
- `title` (required): PR title
- `description` (optional): PR description with markdown support
- `sourceBranch` (required): Source branch name
- `destinationBranch` (required): Destination branch name (typically `main` or `master`)
- `reviewers` (optional): Array of reviewer account UUIDs in format `{account-uuid}`
- `isDraft` (optional): Create as draft PR (default: `false`)

**Example**:
```json
{
  "repo": "myrepo",
  "title": "Add authentication feature",
  "description": "This PR implements OAuth2 authentication\n\n## Changes\n- Add auth middleware\n- Configure OIDC provider",
  "sourceBranch": "feature/auth",
  "destinationBranch": "main",
  "reviewers": ["{uuid-1}", "{uuid-2}"],
  "isDraft": false
}
```

### 2. UpdatePullRequest

Updates an existing pull request's metadata.

**Parameters**:
- `repo` (required): Repository slug
- `prId` (required): Pull request ID number
- `title` (optional): New PR title
- `description` (optional): New PR description
- `reviewers` (optional): Updated list of reviewer UUIDs
- `isDraft` (optional): Change draft status

**Example**:
```json
{
  "repo": "myrepo",
  "prId": 42,
  "title": "Add authentication feature [WIP]",
  "description": "Still in development...",
  "isDraft": true
}
```

### 3. GetPullRequest

Retrieves details of a specific pull request.

**Parameters**:
- `repo` (required): Repository slug
- `prId` (required): Pull request ID number

**Example**:
```json
{
  "repo": "myrepo",
  "prId": 42
}
```

**Response includes**: Title, description, author, state, reviewers, created date, updated date, and merge status.

### 4. ListPullRequests

Lists pull requests in a repository with optional state filtering.

**Parameters**:
- `repo` (required): Repository slug
- `state` (optional): Filter by state: `OPEN`, `MERGED`, `DECLINED`, `SUPERSEDED` (default: `OPEN`)

**Example - Get all open PRs**:
```json
{
  "repo": "myrepo",
  "state": "OPEN"
}
```

**Example - Get merged PRs**:
```json
{
  "repo": "myrepo",
  "state": "MERGED"
}
```

## Transport Modes

### Stdio Transport (Recommended for MCP Clients)

Communicates via standard input/output streams. Ideal for integration with MCP clients like Claude Desktop.

```bash
# Run server
bitbucket-mcp --transport=stdio

# Or explicitly:
bitbucket-mcp --transport=stdio
```

**Pros**:
- Simple process-based integration
- No network overhead
- Direct stderr for logging
- Perfect for desktop applications

**Cons**:
- Cannot be accessed remotely
- Single client per process

### HTTP Transport

Uses HTTP with Server-Sent Events (SSE) for bidirectional communication.

```bash
# Run server
bitbucket-mcp --transport=http

# Server listens on http://localhost:5000 by default
export ASPNETCORE_URLS=http://localhost:5000
bitbucket-mcp --transport=http
```

**Configuration for HTTP clients**:
```json
{
  "mcpServers": {
    "bitbucket": {
      "url": "http://localhost:5000/",
      "transport": {
        "type": "http"
      }
    }
  }
}
```

**Pros**:
- Network-accessible
- Multiple concurrent clients
- Can be deployed as a service

**Cons**:
- Requires port forwarding for remote access
- Network latency considerations

## API Mapping

The MCP server wraps the following Bitbucket Cloud REST API v2.0 endpoints:

| MCP Tool | API Method | Endpoint |
|----------|-----------|----------|
| `CreatePullRequest` | POST | `/repositories/{workspace}/{repo}/pullrequests` |
| `GetPullRequest` | GET | `/repositories/{workspace}/{repo}/pullrequests/{pr_id}` |
| `UpdatePullRequest` | PUT | `/repositories/{workspace}/{repo}/pullrequests/{pr_id}` |
| `ListPullRequests` | GET | `/repositories/{workspace}/{repo}/pullrequests` |

**Base URL**: `https://api.bitbucket.org/2.0`

## Troubleshooting

### Common Issues

#### Issue: `401 Unauthorized` errors

**Solution**:
- Verify `BITBUCKET_USERNAME` is set to your username (not email)
- Confirm `BITBUCKET_APP_PASSWORD` is correct and not expired
- Check app password has required permissions (PR Read/Write, Repo Read)
- Ensure `BITBUCKET_WORKSPACE` matches your workspace slug

#### Issue: Package not found when installing

**Solution**:
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Install with explicit source
dotnet tool install --global Kubis1982.Atlassian.Bitbucket.MCP --source https://api.nuget.org/v3/index.json
```

#### Issue: Command not found after installation

**Solution**:
```bash
# Verify tool was installed
dotnet tool list --global

# Check .dotnet/tools is in PATH
# On Windows: add %USERPROFILE%\.dotnet\tools to PATH
# On Unix: add ~/.dotnet/tools to PATH

# Or use full path
~/.dotnet/tools/bitbucket-mcp --transport=stdio
```

#### Issue: MCP client cannot connect to server

**Solution**:
- Verify server is running: `bitbucket-mcp --transport=stdio`
- For HTTP mode, check port is not already in use: `netstat -an | grep 5000`
- Verify environment variables are set before starting the server
- Check firewall allows connections on the port
- Enable logging for diagnostics: `ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT=Debug`

#### Issue: `dotnet: command not found`

**Solution**:
- Install .NET SDK from https://dotnet.microsoft.com/download
- Or use Docker container: `docker run -it mcr.microsoft.com/dotnet/runtime:10.0`

### Debug Logging

Enable detailed logging to diagnose issues:

```bash
# Set logging level
export ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT=Debug

# Or on Windows (PowerShell)
$env:ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT="Debug"

# Run server
bitbucket-mcp --transport=stdio
```

Logs will be written to:
- **Stdio mode**: stderr
- **HTTP mode**: console output

## Building from Source

```bash
# Clone the repository
git clone https://github.com/kubis1982/BitbucketMCP.git
cd BitbucketMCP

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests (if available)
dotnet test

# Publish for local installation
dotnet publish -c Release -o ./publish

# Install locally
dotnet tool install --global --add-source ./publish Kubis1982.Atlassian.Bitbucket.MCP
```

## Development

### Project Structure

```
BitbucketMCP/
├── src/BitbucketMCP/
│   ├── Configuration/          # Configuration models
│   │   └── BitbucketConfig.cs
│   ├── Models/                 # Data models for API responses
│   ├── Tools/                  # MCP tool implementations
│   │   ├── CreatePullRequestTool.cs
│   │   ├── GetPullRequestTool.cs
│   │   ├── UpdatePullRequestTool.cs
│   │   └── ListPullRequestsTool.cs
│   ├── Program.cs              # Application entry point + DI + transport setup
│   ├── McpServiceCollectionExtensions.cs  # Extension methods for DI
│   ├── GlobalUsings.cs         # Global using statements
│   └── BitbucketMCP.csproj     # Project configuration (PackageType: McpServer)
└── README.md
```

### Key Dependencies

- **ModelContextProtocol** (v1.2.0+): Core MCP protocol implementation
- **ModelContextProtocol.AspNetCore** (v1.2.0+): ASP.NET Core transport bindings
- **Kubis1982.Atlassian.Bitbucket.RestClient.v2**: Type-safe Bitbucket API client

## NuGet Package Configuration

The project is configured as an MCP server package with:

```xml
<PropertyGroup>
  <PackageType>McpServer</PackageType>
  <PackAsTool>true</PackAsTool>
  <ToolCommandName>bitbucket-mcp</ToolCommandName>
  <McpServerJsonTemplateFile>.mcp/server.json</McpServerJsonTemplateFile>
</PropertyGroup>
```

When packaged:
- Executable is installable via `dotnet tool install`
- `server.json` metadata is included in the package
- Tool command `bitbucket-mcp` is available after installation

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

## Acknowledgments

- Built with [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- Uses [.NET 10](https://dot.net)
