using BitbucketMCP.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using ModelContextProtocol.Server;
using System.Text;
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

// Register authentication provider based on configuration (inline implementations)
builder.Services.AddSingleton<IAuthenticationProvider>(sp =>
{
    var bitbucketConfig = sp.GetRequiredService<BitbucketConfig>();
    
    return bitbucketConfig.AuthType switch
    {
        "app_password" => new BasicAuthProvider(bitbucketConfig.Username!, bitbucketConfig.AppPassword!),
        "oauth_token" => new BearerTokenAuthProvider(bitbucketConfig.Token!),
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

// Register Kiota BitbucketApiClient (used directly by Tools)
builder.Services.AddSingleton<KiotaClient>(sp =>
{
    var requestAdapter = sp.GetRequiredService<HttpClientRequestAdapter>();
    return new KiotaClient(requestAdapter);
});

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

// Inline authentication providers (no longer in separate file)

/// <summary>
/// Authentication provider for Basic Authentication (App Password)
/// </summary>
file class BasicAuthProvider : IAuthenticationProvider
{
    private readonly string _username;
    private readonly string _appPassword;

    public BasicAuthProvider(string username, string appPassword)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _appPassword = appPassword ?? throw new ArgumentNullException(nameof(appPassword));
    }

    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_appPassword}"));
        request.Headers.Add("Authorization", $"Basic {credentials}");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Authentication provider for Bearer Token (OAuth)
/// </summary>
file class BearerTokenAuthProvider : IAuthenticationProvider
{
    private readonly string _token;

    public BearerTokenAuthProvider(string token)
    {
        _token = token ?? throw new ArgumentNullException(nameof(token));
    }

    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        request.Headers.Add("Authorization", $"Bearer {_token}");
        return Task.CompletedTask;
    }
}
