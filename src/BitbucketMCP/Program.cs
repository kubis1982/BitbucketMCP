using BitbucketMCP.Configuration;
using BitbucketMCP.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using ModelContextProtocol.Server;
using KiotaClient = BitbucketMCP.Generated.BitbucketApiClient;

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

// Register authentication provider based on configuration
builder.Services.AddSingleton<IAuthenticationProvider>(sp =>
{
    var bitbucketConfig = sp.GetRequiredService<BitbucketConfig>();
    
    return bitbucketConfig.AuthType switch
    {
        "app_password" => new BasicAuthenticationProvider(
            bitbucketConfig.Username!,
            bitbucketConfig.AppPassword!),
        "oauth_token" => new BearerTokenAuthenticationProvider(
            bitbucketConfig.Token!),
        _ => throw new InvalidOperationException($"Unknown auth type: {bitbucketConfig.AuthType}")
    };
});

// Register HttpClient for Kiota
builder.Services.AddHttpClient("Kiota", client =>
{
    client.BaseAddress = new Uri("https://api.bitbucket.org/2.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register Kiota RequestAdapter
builder.Services.AddSingleton(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var authProvider = sp.GetRequiredService<IAuthenticationProvider>();
    var httpClient = httpClientFactory.CreateClient("Kiota");
    
    return new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
});

// Register Kiota BitbucketApiClient
builder.Services.AddSingleton<KiotaClient>(sp =>
{
    var requestAdapter = sp.GetRequiredService<HttpClientRequestAdapter>();
    return new KiotaClient(requestAdapter);
});

// Register BitbucketApiClient wrapper
builder.Services.AddSingleton<BitbucketApiClient>();

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
