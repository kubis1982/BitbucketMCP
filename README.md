# Bitbucket MCP Server

A Model Context Protocol (MCP) server that provides a wrapper for Bitbucket Cloud REST API, enabling Pull Request operations through AI-powered tools.

## Features

- **MCP Protocol Support**: Full implementation of Model Context Protocol over stdio transport
- **Pull Request Management**: 
  - Create pull requests with customizable settings
  - Update PR title, description, and reviewers
  - Toggle between open and draft status
  - Retrieve PR details
- **Flexible Authentication**: Support for both Bitbucket app passwords and OAuth 2.0 tokens
- **Docker Support**: Ready-to-use Docker container with multi-stage build
- **Modern .NET**: Built on .NET 10 with latest SDK patterns

## Prerequisites

- Docker (for containerized deployment) OR
- .NET 10 SDK (for local development)
- Bitbucket Cloud account
- Bitbucket app password or OAuth token

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
     BITBUCKET_AUTH_TYPE: "app_password"
     BITBUCKET_USERNAME: "your-username"
     BITBUCKET_APP_PASSWORD: "your-app-password"
   ```
   
   For OAuth token authentication:
   ```yaml
   environment:
     BITBUCKET_AUTH_TYPE: "oauth_token"
     BITBUCKET_TOKEN: "your-oauth-token"
   ```

3. **Run the server**:
   ```bash
   docker-compose up -d
   ```

### Local Development

1. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

2. **Set environment variables**:
   ```bash
   # Windows (PowerShell)
   $env:BITBUCKET_AUTH_TYPE="app_password"
   $env:BITBUCKET_USERNAME="your-username"
   $env:BITBUCKET_APP_PASSWORD="your-app-password"
   
   # Linux/Mac
   export BITBUCKET_AUTH_TYPE=app_password
   export BITBUCKET_USERNAME=your-username
   export BITBUCKET_APP_PASSWORD=your-app-password
   ```

3. **Run the server**:
   ```bash
   dotnet run --project src/BitbucketMCP
   ```

## Configuration

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `BITBUCKET_AUTH_TYPE` | Yes | Authentication method: `app_password` or `oauth_token` |
| `BITBUCKET_USERNAME` | Conditional | Required for `app_password` auth |
| `BITBUCKET_APP_PASSWORD` | Conditional | Required for `app_password` auth |
| `BITBUCKET_TOKEN` | Conditional | Required for `oauth_token` auth |
| `BITBUCKET_WORKSPACE` | No | Default workspace for operations |
| `LOG_LEVEL` | No | Logging level (Trace, Debug, Information, Warning, Error) |

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
- `workspace` (required): Workspace ID or slug
- `repository` (required): Repository name
- `title` (required): PR title
- `description`: PR description
- `sourceBranch` (required): Source branch name
- `destinationBranch` (required): Destination branch name (default: main)
- `reviewers`: Array of reviewer account IDs
- `isDraft`: Create as draft PR (default: false)

**Example**:
```json
{
  "workspace": "myworkspace",
  "repository": "myrepo",
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
- `workspace` (required): Workspace ID or slug
- `repository` (required): Repository name
- `pullRequestId` (required): PR ID number
- `title`: New PR title
- `description`: New PR description
- `reviewers`: Updated array of reviewer account IDs
- `isDraft`: Change draft status

**Example**:
```json
{
  "workspace": "myworkspace",
  "repository": "myrepo",
  "pullRequestId": 123,
  "title": "Updated title",
  "description": "Updated description"
}
```

### 3. GetPullRequest

Retrieves details of a pull request.

**Parameters**:
- `workspace` (required): Workspace ID or slug
- `repository` (required): Repository name
- `pullRequestId` (required): PR ID number

**Example**:
```json
{
  "workspace": "myworkspace",
  "repository": "myrepo",
  "pullRequestId": 123
}
```

## API Mapping

The server wraps the following Bitbucket Cloud REST API v2.0 endpoints:

| MCP Tool | HTTP Method | Bitbucket API Endpoint |
|----------|-------------|------------------------|
| CreatePullRequest | POST | `/repositories/{workspace}/{repo}/pullrequests` |
| UpdatePullRequest | PUT | `/repositories/{workspace}/{repo}/pullrequests/{pr_id}` |
| GetPullRequest | GET | `/repositories/{workspace}/{repo}/pullrequests/{pr_id}` |

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

# Run the container
docker run -it --rm \
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
- Verify server is running with stdio transport
- Check logs output to stderr for error messages
- Ensure no other process is writing to stdout/stderr
- Validate JSON-RPC message format

## Development

### Project Structure

```
BitbucketMCP/
├── src/
│   └── BitbucketMCP/
│       ├── Configuration/      # Configuration models
│       ├── Models/             # Data models
│       ├── Services/           # Bitbucket API client
│       ├── Tools/              # MCP tool implementations
│       └── Program.cs          # Application entry point
├── docs/                       # Documentation
├── Dockerfile                  # Docker configuration
├── docker-compose.yml          # Docker Compose setup
└── README.md                   # This file
```

### Adding New Tools

1. Create a new class in `Tools/` directory
2. Add `[McpServerToolType]` attribute to the class
3. Create methods with `[McpServerTool]` attribute
4. Add parameter descriptions with `[Description]` attribute
5. Implement business logic using `BitbucketApiClient`

Example:
```csharp
[McpServerToolType]
public class MyCustomTool
{
    [McpServerTool, Description("Does something useful")]
    public async Task<string> DoSomething(
        [Description("Input parameter")] string input)
    {
        // Implementation
        return "Result";
    }
}
```

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
- Powered by [Bitbucket Cloud REST API v2.0](https://developer.atlassian.com/cloud/bitbucket/rest/)
- Uses [.NET 10](https://dot.net)

## Support

For issues, questions, or contributions, please visit:
- GitHub Issues: https://github.com/kubis1982/BitbucketMCP/issues
- Bitbucket API Documentation: https://developer.atlassian.com/cloud/bitbucket/rest/

## Version

Current version: 1.0.0