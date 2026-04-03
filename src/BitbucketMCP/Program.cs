using BitbucketMCP.Configuration;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using ModelContextProtocol.Server;
using System.Text;

// Parse transport argument (--transport=stdio or --transport=http)
var transport = args
    .FirstOrDefault(arg => arg.StartsWith("--transport=", StringComparison.OrdinalIgnoreCase))?
    .Split('=', 2)
    .LastOrDefault()
    ?.ToLowerInvariant() ?? "http";

if (transport != "http" && transport != "stdio")
{
    Console.Error.WriteLine($"Error: Invalid transport '{transport}'. Valid options are: http, stdio");
    Environment.Exit(1);
}

Console.WriteLine($"Starting BitbucketMCP with {transport} transport...");

// Read and validate Bitbucket configuration from environment variables
var config = new BitbucketConfig
{
    Username = Environment.GetEnvironmentVariable("BITBUCKET_USERNAME") ?? string.Empty,
    AppPassword = Environment.GetEnvironmentVariable("BITBUCKET_APP_PASSWORD") ?? string.Empty,
    Workspace = Environment.GetEnvironmentVariable("BITBUCKET_WORKSPACE") ?? string.Empty
};

try
{
    config.Validate();
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Configuration error: {ex.Message}");
    Environment.Exit(1);
}

if (transport == "stdio")
{
    // Stdio transport setup
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.RegisterServices(config);
    
    // Configure MCP Server with stdio transport
    builder.Services.AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly();
    
    await builder.Build().RunAsync();
}
else // HTTP transport
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.RegisterServices(config);
    
    // Configure MCP Server with HTTP transport
    builder.Services.AddMcpServer()
        .WithHttpTransport()
        .WithToolsFromAssembly();
    
    var app = builder.Build();
    
    // Map MCP endpoints
    app.MapMcp();
    
    app.Run();
}