using System;
using System.Threading.Tasks;
using BitbucketMCP.Configuration;
using BitbucketMCP.Tools;
using Xunit;

namespace BitbucketMCP.Tests
{
    // Creates a throwaway draft PR via CreatePullRequestTool, then confirms it shows
    // up when listing OPEN pull requests with ListPullRequestsTool against the real
    // Bitbucket Cloud API. Skipped unless credentials + a target repo are provided.
    //
    // Run explicitly, e.g.:
    //   BITBUCKET_USERNAME=... BITBUCKET_APP_PASSWORD=... BITBUCKET_WORKSPACE=... \
    //   BITBUCKET_TEST_REPO=my-repo BITBUCKET_TEST_SOURCE_BRANCH=develop BITBUCKET_TEST_DEST_BRANCH=main \
    //   dotnet test --filter "FullyQualifiedName~ListPullRequestsIntegrationTests"
    [Trait("Category", "Integration")]
    public class ListPullRequestsIntegrationTests
    {
        [Fact]
        public async Task ListPullRequests_IncludesPreviouslyCreatedPrFromRealRepo()
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
                    "list_pull_requests");

                var listTool = new ListPullRequestsTool(client, config);
                var result = await listTool.ListPullRequests(repo, state: "OPEN");

                Assert.NotNull(result);
                Assert.Contains(result, pr => pr.Id == created.Id);

                Console.WriteLine($"Found {result.Count} open pull request(s) in '{repo}':");
                foreach (var pr in result)
                {
                    Console.WriteLine($"  #{pr.Id} {pr.Title} -> {pr.Url}");
                }
            }
        }
    }
}
