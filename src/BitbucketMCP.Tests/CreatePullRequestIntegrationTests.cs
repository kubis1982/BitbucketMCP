using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BitbucketMCP.Configuration;
using BitbucketMCP.Tools;
using Kubis1982.Atlassian.Bitbucket.RestClient;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
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
            var username = Environment.GetEnvironmentVariable("BITBUCKET_USERNAME");
            var appPassword = Environment.GetEnvironmentVariable("BITBUCKET_APP_PASSWORD");
            var workspace = Environment.GetEnvironmentVariable("BITBUCKET_WORKSPACE");
            var repo = Environment.GetEnvironmentVariable("BITBUCKET_TEST_REPO");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(appPassword) ||
                string.IsNullOrWhiteSpace(workspace) || string.IsNullOrWhiteSpace(repo))
            {
                Assert.Skip(
                    "Set BITBUCKET_USERNAME, BITBUCKET_APP_PASSWORD, BITBUCKET_WORKSPACE and " +
                    "BITBUCKET_TEST_REPO to run this test against a real Bitbucket account.");
                return;
            }

            var sourceBranch = Environment.GetEnvironmentVariable("BITBUCKET_TEST_SOURCE_BRANCH") ?? "develop";
            var destinationBranch = Environment.GetEnvironmentVariable("BITBUCKET_TEST_DEST_BRANCH") ?? "main";

            var config = new BitbucketConfig
            {
                Username = username,
                AppPassword = appPassword,
                Workspace = workspace
            };

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.bitbucket.org/2.0"),
                Timeout = TimeSpan.FromSeconds(30)
            };
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{appPassword}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
            var client = new BitbucketRestClient(requestAdapter);
            var tool = new CreatePullRequestTool(client, config);

            var title = $"[test] addDefaultReviewers {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}Z";

            var result = await tool.CreatePullRequest(
                repo: repo,
                title: title,
                description: "Utworzone automatycznie przez CreatePullRequestIntegrationTests w celu weryfikacji " +
                              "addDefaultReviewers. Można bezpiecznie odrzucić (decline) ten PR.",
                sourceBranch: sourceBranch,
                destinationBranch: destinationBranch,
                isDraft: true,
                addDefaultReviewers: true);

            Assert.NotNull(result);
            Assert.Equal(title, result.Title);
            Assert.False(string.IsNullOrWhiteSpace(result.Url));

            Console.WriteLine($"PR utworzony: {result.Url}");
            Console.WriteLine(
                $"Reviewerzy (powinni zawierać domyślnych reviewerów repo '{repo}'): " +
                (result.Reviewers is { Count: > 0 } reviewers ? string.Join(", ", reviewers) : "brak"));
        }
    }
}
