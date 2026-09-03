using System;
using System.Threading.Tasks;
using BitbucketMCP.Configuration;
using BitbucketMCP.Tools;
using Xunit;

namespace BitbucketMCP.Tests
{
    // Creates a throwaway draft PR via CreatePullRequestTool, then fetches it with
    // GetPullRequestTool against the real Bitbucket Cloud API, so the mapping can be
    // verified against a real response instead of a mock. Skipped unless credentials
    // + a target repo are provided.
    //
    // Run explicitly, e.g.:
    //   BITBUCKET_USERNAME=... BITBUCKET_APP_PASSWORD=... BITBUCKET_WORKSPACE=... \
    //   BITBUCKET_TEST_REPO=my-repo BITBUCKET_TEST_SOURCE_BRANCH=develop BITBUCKET_TEST_DEST_BRANCH=main \
    //   dotnet test --filter "FullyQualifiedName~GetPullRequestIntegrationTests"
    [Trait("Category", "Integration")]
    public class GetPullRequestIntegrationTests
    {
        [Fact]
        public async Task GetPullRequest_ReturnsPreviouslyCreatedPrFromRealRepo()
        {
            if (!BitbucketIntegrationTestFactory.TryGetCredentials(out var username, out var appPassword, out var workspace))
            {
                Assert.Skip(BitbucketIntegrationTestFactory.MissingCredentialsMessage);
                return;
            }

            var repo = BitbucketIntegrationTestFactory.GetTestRepo();
            if (string.IsNullOrWhiteSpace(repo))
            {
                Assert.Skip(BitbucketIntegrationTestFactory.MissingRepoMessage);
                return;
            }

            var config = new BitbucketConfig
            {
                Username = username,
                AppPassword = appPassword,
                Workspace = workspace
            };

            var (client, httpClient) = BitbucketIntegrationTestFactory.CreateClient(username, appPassword);
            using (httpClient)
            {
                var createTool = new CreatePullRequestTool(client, config);
                var created = await BitbucketIntegrationTestFactory.CreateDraftPullRequestAsync(
                    createTool,
                    repo,
                    BitbucketIntegrationTestFactory.GetSourceBranch(),
                    BitbucketIntegrationTestFactory.GetDestinationBranch(),
                    "get_pull_request");

                Assert.NotNull(created.Id);

                var getTool = new GetPullRequestTool(client, config);
                var result = await getTool.GetPullRequest(repo, created.Id!.Value);

                Assert.NotNull(result);
                Assert.Equal(created.Id, result.Id);
                Assert.Equal(created.Title, result.Title);

                Console.WriteLine($"PR #{result.Id}: {result.Title} ({result.State}) -> {result.Url}");
            }
        }
    }
}
