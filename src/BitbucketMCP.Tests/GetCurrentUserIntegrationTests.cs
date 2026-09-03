using System;
using System.Threading.Tasks;
using BitbucketMCP.Tools;
using Xunit;

namespace BitbucketMCP.Tests
{
    // Hits the real Bitbucket Cloud API so a human can confirm get_current_user
    // returns the account behind the configured credentials. Skipped unless
    // credentials are provided.
    //
    // Run explicitly, e.g.:
    //   BITBUCKET_USERNAME=... BITBUCKET_APP_PASSWORD=... BITBUCKET_WORKSPACE=... \
    //   dotnet test --filter "FullyQualifiedName~GetCurrentUserIntegrationTests"
    [Trait("Category", "Integration")]
    public class GetCurrentUserIntegrationTests
    {
        [Fact]
        public async Task GetCurrentUser_ReturnsAuthenticatedAccountFromRealApi()
        {
            if (!BitbucketIntegrationTestFactory.TryGetCredentials(out var username, out var appPassword, out _))
            {
                Assert.Skip(BitbucketIntegrationTestFactory.MissingCredentialsMessage);
                return;
            }

            var (client, httpClient) = BitbucketIntegrationTestFactory.CreateClient(username, appPassword);
            using (httpClient)
            {
                var tool = new GetCurrentUserTool(client);

                var result = await tool.GetCurrentUser();

                Assert.NotNull(result);
                Assert.False(string.IsNullOrWhiteSpace(result.Uuid));

                Console.WriteLine($"Current user: {result.DisplayName} ({result.Uuid})");
                Console.WriteLine($"Account type: {result.AccountType}, created on: {result.CreatedOn}");
            }
        }
    }
}
