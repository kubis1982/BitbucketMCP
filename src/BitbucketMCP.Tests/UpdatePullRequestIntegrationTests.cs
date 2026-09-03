using System;
using System.Threading.Tasks;
using BitbucketMCP.Configuration;
using BitbucketMCP.Tools;
using Xunit;

namespace BitbucketMCP.Tests
{
    // Creates a throwaway draft PR via CreatePullRequestTool, then updates its title
    // and description with UpdatePullRequestTool against the real Bitbucket Cloud
    // API, so a human can open it and confirm the update actually took effect.
    // Skipped unless credentials + a target repo are provided.
    //
    // Run explicitly, e.g.:
    //   BITBUCKET_USERNAME=... BITBUCKET_APP_PASSWORD=... BITBUCKET_WORKSPACE=... \
    //   BITBUCKET_TEST_REPO=my-repo BITBUCKET_TEST_SOURCE_BRANCH=develop BITBUCKET_TEST_DEST_BRANCH=main \
    //   dotnet test --filter "FullyQualifiedName~UpdatePullRequestIntegrationTests"
    [Trait("Category", "Integration")]
    public class UpdatePullRequestIntegrationTests
    {
        [Fact]
        public async Task UpdatePullRequest_ChangesTitleAndDescriptionOnRealRepo()
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
                    "update_pull_request");

                Assert.NotNull(created.Id);

                var updateTool = new UpdatePullRequestTool(client, config);
                var updatedTitle = $"{created.Title} [updated]";
                var updatedDescription = "Updated by UpdatePullRequestIntegrationTests. Safe to decline this PR.";

                var updated = await updateTool.UpdatePullRequest(
                    repo: repo,
                    prId: created.Id!.Value,
                    title: updatedTitle,
                    description: updatedDescription);

                Assert.NotNull(updated);
                Assert.Equal(updatedTitle, updated.Title);
                Assert.Equal(updatedDescription, updated.Description);

                Console.WriteLine($"PR updated: {updated.Url}");
            }
        }
    }
}
