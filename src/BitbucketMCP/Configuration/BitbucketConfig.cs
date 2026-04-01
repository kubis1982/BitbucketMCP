namespace BitbucketMCP.Configuration;

public class BitbucketConfig
{
    public string AuthType { get; set; } = "app_password";
    public string? Username { get; set; }
    public string? AppPassword { get; set; }
    public string? Token { get; set; }
    public string? DefaultWorkspace { get; set; }

    public void Validate()
    {
        if (AuthType == "app_password")
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(AppPassword))
            {
                throw new InvalidOperationException(
                    "For app_password authentication, both BITBUCKET_USERNAME and BITBUCKET_APP_PASSWORD must be set.");
            }
        }
        else if (AuthType == "oauth_token")
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                throw new InvalidOperationException(
                    "For oauth_token authentication, BITBUCKET_TOKEN must be set.");
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"Invalid BITBUCKET_AUTH_TYPE: {AuthType}. Must be 'app_password' or 'oauth_token'.");
        }
    }
}
