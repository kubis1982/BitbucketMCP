namespace BitbucketMCP.Configuration;

public class BitbucketConfig
{
    public required string Username { get; set; }
    public required string AppPassword { get; set; }
    public required string Workspace { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            throw new InvalidOperationException(
                "BITBUCKET_USERNAME must be set.");
        }

        if (string.IsNullOrWhiteSpace(AppPassword))
        {
            throw new InvalidOperationException(
                "BITBUCKET_APP_PASSWORD must be set.");
        }

        if (string.IsNullOrWhiteSpace(Workspace))
        {
            throw new InvalidOperationException(
                "BITBUCKET_WORKSPACE must be set.");
        }
    }
}
