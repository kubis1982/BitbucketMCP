#nullable enable

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BitbucketMCP.Models;
using BitbucketMCP.Tools;
using Kubis1982.Atlassian.Bitbucket.RestClient;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace BitbucketMCP.Tests
{
    // Shared setup for tests that hit the real Bitbucket Cloud API instead of a mock,
    // so a human can open the resulting pull requests and confirm the tool under test
    // behaved correctly. See any *IntegrationTests class for the environment variables
    // involved and how to run these from the Visual Studio Test Explorer.
    internal static class BitbucketIntegrationTestFactory
    {
        public const string MissingCredentialsMessage =
            "Set BITBUCKET_USERNAME, BITBUCKET_APP_PASSWORD and BITBUCKET_WORKSPACE to run this test against a real Bitbucket account.";

        public const string MissingRepoMessage =
            "Set BITBUCKET_TEST_REPO to a real repository slug to run this test against a real Bitbucket account.";

        public static bool TryGetCredentials(out string username, out string appPassword, out string workspace)
        {
            username = Environment.GetEnvironmentVariable("BITBUCKET_USERNAME") ?? string.Empty;
            appPassword = Environment.GetEnvironmentVariable("BITBUCKET_APP_PASSWORD") ?? string.Empty;
            workspace = Environment.GetEnvironmentVariable("BITBUCKET_WORKSPACE") ?? string.Empty;

            return !string.IsNullOrWhiteSpace(username)
                && !string.IsNullOrWhiteSpace(appPassword)
                && !string.IsNullOrWhiteSpace(workspace);
        }

        public static string? GetTestRepo() => Environment.GetEnvironmentVariable("BITBUCKET_TEST_REPO");

        public static string GetSourceBranch() => Environment.GetEnvironmentVariable("BITBUCKET_TEST_SOURCE_BRANCH") ?? "develop";

        public static string GetDestinationBranch() => Environment.GetEnvironmentVariable("BITBUCKET_TEST_DEST_BRANCH") ?? "main";

        public static (BitbucketRestClient Client, HttpClient HttpClient) CreateClient(string username, string appPassword)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.bitbucket.org/2.0"),
                Timeout = TimeSpan.FromSeconds(30)
            };

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{appPassword}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
            return (new BitbucketRestClient(requestAdapter), httpClient);
        }

        // Creates a throwaway draft PR via CreatePullRequestTool so other tools can be
        // exercised against real, known state without depending on pre-existing data in
        // the repository. Not cleaned up afterwards on purpose, so it can be inspected
        // on Bitbucket.
        public static async Task<PullResponse> CreateDraftPullRequestAsync(
            CreatePullRequestTool createTool,
            string repo,
            string sourceBranch,
            string destinationBranch,
            string label)
        {
            var title = $"[test] {label} {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}Z";

            return await createTool.CreatePullRequest(
                repo: repo,
                title: title,
                description: $"Created by {label} integration test. Safe to decline.",
                sourceBranch: sourceBranch,
                destinationBranch: destinationBranch,
                isDraft: true);
        }
    }
}
