using System.ComponentModel;
using BitbucketMCP.Models;
using BitbucketMCP.Services;
using ModelContextProtocol.Server;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class CreatePullRequestTool(BitbucketApiClient apiClient)
{
    [McpServerTool("create_pull_request", "Creates a new pull request in a Bitbucket repository")]
    public async Task<string> CreatePullRequest(
        [Description("The workspace slug (e.g., 'myworkspace')")] string workspace,
        [Description("The repository slug (e.g., 'myrepo')")] string repo,
        [Description("The title of the pull request")] string title,
        [Description("The description/body of the pull request")] string? description,
        [Description("The source branch name (the branch to merge from)")] string sourceBranch,
        [Description("The destination branch name (the branch to merge into, usually 'main' or 'master')")] string destinationBranch,
        [Description("Optional array of reviewer account UUIDs in format '{account-id}'")] List<string>? reviewers = null,
        [Description("Whether this is a draft pull request (note: managed via description prefix)")] bool isDraft = false)
    {
        try
        {
            var request = new PullRequestRequest
            {
                Title = title,
                Description = isDraft && !string.IsNullOrEmpty(description) 
                    ? $"[DRAFT] {description}" 
                    : description ?? string.Empty,
                SourceBranch = sourceBranch,
                DestinationBranch = destinationBranch,
                Reviewers = reviewers,
                IsDraft = isDraft
            };

            var result = await apiClient.CreatePullRequest(workspace, repo, request);

            return $"✅ Pull request created successfully!\n\n" +
                   $"ID: {result.Id}\n" +
                   $"Title: {result.Title}\n" +
                   $"State: {result.State}\n" +
                   $"URL: {result.Url}\n" +
                   $"Created: {result.CreatedOn}";
        }
        catch (HttpRequestException ex)
        {
            return $"❌ Failed to create pull request: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"❌ Error: {ex.Message}";
        }
    }
}
