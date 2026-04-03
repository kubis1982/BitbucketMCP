using BitbucketMCP.Configuration;
using BitbucketMCP.Generated;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using System.Text;

internal static class McpServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection service, BitbucketConfig config)
    {
        // Register configuration as singleton
        service.AddSingleton(config);

        // Register authentication provider
        service.AddSingleton<IAuthenticationProvider>(sp =>
        {
            var bitbucketConfig = sp.GetRequiredService<BitbucketConfig>();
            return new BasicAuthProvider(bitbucketConfig.Username, bitbucketConfig.AppPassword);
        });

        // Register HttpClient for Kiota
        service.AddHttpClient("Kiota", client =>
        {
            client.BaseAddress = new Uri("https://api.bitbucket.org/2.0");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register Kiota RequestAdapter
        service.AddSingleton(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var authProvider = sp.GetRequiredService<IAuthenticationProvider>();
            var httpClient = httpClientFactory.CreateClient("Kiota");

            return new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
        });

        // Register Kiota BitbucketApiClient
        service.AddSingleton(sp =>
        {
            var requestAdapter = sp.GetRequiredService<HttpClientRequestAdapter>();
            return new BitbucketApiClient(requestAdapter);
        });

        return service;
    }
}

/// <summary>
/// Authentication provider for Basic Authentication (App Password)
/// </summary>
file class BasicAuthProvider(string username, string appPassword) : IAuthenticationProvider
{
    private readonly string _username = username ?? throw new ArgumentNullException(nameof(username));
    private readonly string _appPassword = appPassword ?? throw new ArgumentNullException(nameof(appPassword));

    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_appPassword}"));
        request.Headers.Add("Authorization", $"Basic {credentials}");
        return Task.CompletedTask;
    }
}
