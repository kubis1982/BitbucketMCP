using System;
using System.Threading.Tasks;
using BitbucketMCP.Configuration;
using BitbucketMCP.Tools;
using Xunit;

namespace BitbucketMCP.Tests
{
    // Hits the real Bitbucket Cloud API instead of a mock, so a human can open the
    // resulting PR and confirm addDefaultReviewers actually attached the repo's
    // default reviewers. Skipped unless credentials + a target repo are provided.
    //
    // Run explicitly, e.g.:
    //   BITBUCKET_USERNAME=... BITBUCKET_APP_PASSWORD=... BITBUCKET_WORKSPACE=... \
    //   BITBUCKET_TEST_REPO=my-repo BITBUCKET_TEST_SOURCE_BRANCH=develop BITBUCKET_TEST_DEST_BRANCH=main \
    //   dotnet test --filter "FullyQualifiedName~CreatePullRequestIntegrationTests"
    [Trait("Category", "Integration")]
    public class CreatePullRequestIntegrationTests
    {
        [Fact]
        public async Task CreatePullRequest_WithAddDefaultReviewers_CreatesDraftPrOnRealRepo()
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

            var sourceBranch = BitbucketIntegrationTestFactory.GetSourceBranch();
            var destinationBranch = BitbucketIntegrationTestFactory.GetDestinationBranch();

            var config = new BitbucketConfig
            {
                Username = username,
                AppPassword = appPassword,
                Workspace = workspace
            };

            var (client, httpClient) = BitbucketIntegrationTestFactory.CreateClient(username, appPassword);
            using (httpClient)
            {
                var tool = new CreatePullRequestTool(client, config);

                var title = $"[test] addDefaultReviewers {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}Z";

                var result = await tool.CreatePullRequest(
                    repo: repo,
                    title: title,
                    description: "Created automatically by CreatePullRequestIntegrationTests to verify " +
                                  "addDefaultReviewers. Safe to decline this PR.",
                    sourceBranch: sourceBranch,
                    destinationBranch: destinationBranch,
                    isDraft: true,
                    addDefaultReviewers: true);

                Assert.NotNull(result);
                Assert.Equal(title, result.Title);
                Assert.False(string.IsNullOrWhiteSpace(result.Url));

                Console.WriteLine($"PR created: {result.Url}");
                Console.WriteLine(
                    $"Reviewers (should include repo '{repo}' default reviewers): " +
                    (result.Reviewers is { Count: > 0 } reviewers ? string.Join(", ", reviewers) : "none"));
            }
        }
    }
}
