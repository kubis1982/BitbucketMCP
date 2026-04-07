# Bitbucket MCP Server - Docker Images

Model Context Protocol (MCP) server for Bitbucket Cloud Pull Request operations. Available in two transport variants optimized for different use cases.

---

## 📟 Stdio Transport Image (Default)

**Image:** `kubis1982/bitbucket-mcp:latest`

A lightweight ASP.NET (.NET 10) Docker image that runs a Model Context Protocol (MCP) server over **stdio** (standard input/output). Designed for process-based integration with MCP clients like Claude Desktop. The image communicates via stdin/stdout and is configured via environment variables.

### Key features

- Multi-stage image based on `mcr.microsoft.com/dotnet/aspnet:10.0`
- MCP over stdio transport (no network ports required)
- Simple configuration via environment variables
- Optimized for Claude Desktop and similar tools
- Non-root user for enhanced security

### Environment variables

| Variable | Required | Description |
|----------|----------|-------------|
| `BITBUCKET_USERNAME` | Yes | Bitbucket username for authentication |
| `BITBUCKET_APP_PASSWORD` | Yes | Bitbucket app password |
| `BITBUCKET_WORKSPACE` | Yes | Bitbucket workspace for operations |

### Quick start

**Run with Docker:**

```bash
docker run -i --rm \
  -e BITBUCKET_USERNAME=your-username \
  -e BITBUCKET_APP_PASSWORD=your-password \
  -e BITBUCKET_WORKSPACE=your-workspace \
  kubis1982/bitbucket-mcp:latest
```

**docker-compose fragment:**

```yaml
services:
  bitbucket-mcp-stdio:
    image: kubis1982/bitbucket-mcp:latest
    stdin_open: true
    environment:
      - BITBUCKET_USERNAME=your-username
      - BITBUCKET_APP_PASSWORD=your-app-password
      - BITBUCKET_WORKSPACE=your-workspace
```

**Claude Desktop configuration:**

```json
{
  "mcpServers": {
    "bitbucket": {
      "command": "docker",
      "args": [
        "run", "-i",
        "-e", "BITBUCKET_USERNAME=your-username",
        "-e", "BITBUCKET_APP_PASSWORD=your-app-password",
        "-e", "BITBUCKET_WORKSPACE=your-workspace",
        "kubis1982/bitbucket-mcp"
      ]
    }
  }
}
```

### Available tags

- `latest` - Latest stdio variant (default)
- `3.0.0` - Version-specific stdio
- `3.0.0-stdio` - Explicit stdio suffix
- `3.x` - Minor version tracking

### Operational notes

- Ensure the Bitbucket app password has **Pull Request** and **Repository** read/write permissions
- No exposed ports (stdio communication via stdin/stdout)
- Run with `-i` (interactive) flag to enable stdin/stdout
- Inspect logs with `docker logs <container-id>` for troubleshooting

---

## 🌐 HTTP Transport Image

**Image:** `kubis1982/bitbucket-mcp:http-latest`

A lightweight ASP.NET (.NET 10) Docker image that runs a Model Context Protocol (MCP) server over **HTTP/SSE** (Server-Sent Events). Designed for containerized deployment (Docker / docker-compose) to enable AI-driven Pull Request operations against Bitbucket Cloud. The image exposes an HTTP/SSE MCP endpoint and is configured via environment variables.

### Key features

- Multi-stage image based on `mcr.microsoft.com/dotnet/aspnet:10.0`
- MCP over HTTP/SSE (Server-Sent Events) on port 8080 inside the container
- Simple configuration via environment variables
- Ready for web-based MCP clients
- Non-root user for enhanced security

### Environment variables

| Variable | Required | Description |
|----------|----------|-------------|
| `BITBUCKET_USERNAME` | Yes | Bitbucket username for authentication |
| `BITBUCKET_APP_PASSWORD` | Yes | Bitbucket app password |
| `BITBUCKET_WORKSPACE` | Yes | Bitbucket workspace for operations |
| `ASPNETCORE_URLS` | No | Server URL (default: `http://+:8080` in container) |

### Exposed port

- `8080/tcp` — MCP HTTP/SSE endpoint (map with `-p 8080:8080`)

### Quick start

**Run with Docker:**

```bash
docker run -d --rm -p 8080:8080 \
  -e BITBUCKET_USERNAME=your-username \
  -e BITBUCKET_APP_PASSWORD=your-password \
  -e BITBUCKET_WORKSPACE=your-workspace \
  kubis1982/bitbucket-mcp:http-latest
```

**docker-compose fragment:**

```yaml
services:
  bitbucket-mcp-http:
    image: kubis1982/bitbucket-mcp:http-latest
    ports:
      - "8080:8080"
    environment:
      - BITBUCKET_USERNAME=your-username
      - BITBUCKET_APP_PASSWORD=your-app-password
      - BITBUCKET_WORKSPACE=your-workspace
```

**Test the endpoint:**

```bash
curl -H "Accept: text/event-stream" http://localhost:8080/
```

### Available tags

- `http-latest` - Latest HTTP variant
- `3.0.0-http` - Version-specific HTTP
- `3.x-http` - Minor version tracking

### Operational notes

- Ensure the Bitbucket app password has **Pull Request** and **Repository** read/write permissions
- MCP clients must send header `Accept: text/event-stream` to connect via HTTP/SSE
- Container runs on port 8080 internally (map to host with `-p 8080:8080`)
- Inspect logs with `docker logs <container-id>` for troubleshooting

---

## 🔐 Authentication Setup

1. Go to Bitbucket Settings → Personal settings → App passwords
2. Click "Create app password"
3. Grant required permissions:
   - **Pull requests**: Read, Write
   - **Repositories**: Read, Write
4. Copy the generated password (shown only once)
5. Use your Bitbucket username and the app password in environment variables

---

## 🛠️ Available MCP Tools

The server exposes the following MCP tools for Bitbucket operations:

- **CreatePullRequest** - Create a new pull request
- **UpdatePullRequest** - Update an existing pull request
- **GetPullRequest** - Retrieve pull request details
- **ListPullRequests** - List pull requests with optional filtering

For detailed tool documentation, see the [GitHub repository](https://github.com/kubis1982/BitbucketMCP).

---

## 📦 Image Metadata

- **Base image:** `mcr.microsoft.com/dotnet/aspnet:10.0`
- **Platforms:** `linux/amd64`
- **License:** MIT
- **Source code:** https://github.com/kubis1982/BitbucketMCP
- **Issues:** https://github.com/kubis1982/BitbucketMCP/issues
- **Documentation:** https://github.com/kubis1982/BitbucketMCP#readme

---

## 🆘 Troubleshooting

### Authentication Errors

**Problem:** `401 Unauthorized` errors

**Solution:**
- Verify your credentials are correct
- For app passwords, ensure you're using your Bitbucket username (not email)
- Check that app password has required permissions

### Connection Issues (HTTP variant)

**Problem:** Cannot connect to the MCP endpoint

**Solution:**
- Verify the container is running: `docker ps`
- Check port mapping is correct: `-p 8080:8080`
- Ensure firewall allows connections on port 8080
- Verify MCP client sends `Accept: text/event-stream` header

### Stdio Communication Issues

**Problem:** No response from stdio container

**Solution:**
- Ensure container is run with `-i` (interactive) flag
- Check environment variables are set correctly
- Verify stdin/stdout are not being buffered by your client
- Review container logs: `docker logs <container-id>`

---

## 📝 Version Information

**Current version:** 3.0.0

For release notes and changelog, visit: https://github.com/kubis1982/BitbucketMCP/releases

---

## 🤝 Support & Contributing

- **GitHub Issues:** https://github.com/kubis1982/BitbucketMCP/issues
- **Bitbucket API Docs:** https://developer.atlassian.com/cloud/bitbucket/rest/

---

Built with ❤️ using [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) and [Microsoft Kiota](https://github.com/microsoft/kiota)
