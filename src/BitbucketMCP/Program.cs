using BitbucketMCP.Configuration;
using BitbucketMCP.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Read and validate Bitbucket configuration from environment variables
var config = new BitbucketConfig
{
    AuthType = Environment.GetEnvironmentVariable("BITBUCKET_AUTH_TYPE") ?? "app_password",
    Username = Environment.GetEnvironmentVariable("BITBUCKET_USERNAME"),
    AppPassword = Environment.GetEnvironmentVariable("BITBUCKET_APP_PASSWORD"),
    Token = Environment.GetEnvironmentVariable("BITBUCKET_TOKEN"),
    DefaultWorkspace = Environment.GetEnvironmentVariable("BITBUCKET_DEFAULT_WORKSPACE")
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

// Register configuration as singleton
builder.Services.AddSingleton(config);

// Register HttpClient with BitbucketApiClient
builder.Services.AddHttpClient<BitbucketApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.bitbucket.org/2.0");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register BitbucketApiClient as singleton
builder.Services.AddSingleton<BitbucketApiClient>();

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
