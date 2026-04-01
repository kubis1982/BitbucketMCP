using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Text;

namespace BitbucketMCP.Services;

/// <summary>
/// Authentication provider for Basic Authentication (App Password)
/// </summary>
public class BasicAuthenticationProvider(string username, string appPassword) : IAuthenticationProvider
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

/// <summary>
/// Authentication provider for Bearer Token (OAuth)
/// </summary>
public class BearerTokenAuthenticationProvider(string token) : IAuthenticationProvider
{
    private readonly string _token = token ?? throw new ArgumentNullException(nameof(token));

    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        request.Headers.Add("Authorization", $"Bearer {_token}");
        return Task.CompletedTask;
    }
}
