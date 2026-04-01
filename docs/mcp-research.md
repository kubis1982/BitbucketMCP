# MCP .NET Implementation Research

## Official MCP C# SDK

**Recommended Approach:** Use the official MCP C# SDK from Microsoft and Anthropic

### NuGet Packages

We will use **ModelContextProtocol** package which includes:
- Full hosting and dependency injection support
- Automatic tool/prompt/resource discovery
- stdio transport for console applications
- Support for .NET 8/9/10

**Package:** `ModelContextProtocol` (latest version)
**Repository:** https://github.com/modelcontextprotocol/csharp-sdk
**Documentation:** https://csharp.sdk.modelcontextprotocol.io/

### Key Features for Our Implementation

1. **Stdio Transport** - Perfect for MCP server communication
2. **Attribute-based Tool Definition** - Easy tool registration using `[McpServerTool]` attribute
3. **Dependency Injection** - Native .NET hosting support
4. **Logging** - Built-in logging to stderr (required for stdio transport)

### Implementation Pattern

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr for stdio transport
builder.Logging.AddConsole(options => 
{ 
    options.LogToStandardErrorThreshold = LogLevel.Trace; 
});

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
```

### Tool Definition Pattern

```csharp
[McpServerToolType]
public static class BitbucketTools
{
    [McpServerTool, Description("Creates a new pull request")]
    public static async Task<PullRequestResult> CreatePullRequest(
        [Description("Workspace ID")] string workspace,
        [Description("Repository name")] string repository,
        [Description("PR title")] string title,
        // ... other parameters
    )
    {
        // Implementation
    }
}
```

### Required Dependencies

- `ModelContextProtocol` - Main MCP SDK package
- `System.Net.Http` - For Bitbucket REST API calls (built-in)
- `System.Text.Json` - For JSON serialization (built-in)
- `Microsoft.Extensions.Configuration.EnvironmentVariables` - For environment variable configuration
- `Microsoft.Extensions.Http` - For HttpClient factory pattern

## Decision

Use official MCP C# SDK with:
- Console application (not ASP.NET Core) with stdio transport
- Attribute-based tool registration
- .NET 10 hosting model
- Built-in DI and logging infrastructure
