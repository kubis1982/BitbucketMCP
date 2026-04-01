using System.ComponentModel;
using BitbucketMCP.Models;
using BitbucketMCP.Services;
using ModelContextProtocol.Server;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class UpdatePullRequestTool(BitbucketApiClient apiClient)
{
    [McpServerTool(Name = "update_pull_request")]
    [Description("Updates an existing pull request in a Bitbucket repository")]
    public async Task<string> UpdatePullRequest(
        [Description("The workspace slug (e.g., 'myworkspace')")] string workspace,
        [Description("The repository slug (e.g., 'myrepo')")] string repo,
        [Description("The pull request ID number")] int prId,
        [Description("The new title for the pull request (optional)")] string? title = null,
        [Description("The new description/body for the pull request (optional)")] string? description = null,
        [Description("Updated array of reviewer account UUIDs in format '{account-id}' (optional)")] List<string>? reviewers = null,
        [Description("Update draft status (note: managed via description prefix)")] bool? isDraft = null)
    {
        try
        {
            var request = new UpdatePullRequestRequest
            {
                Title = title,
                Description = description,
                Reviewers = reviewers,
                IsDraft = isDraft
            };

            var result = await apiClient.UpdatePullRequest(workspace, repo, prId, request);

            return $"✅ Pull request updated successfully!\n\n" +
                   $"ID: {result.Id}\n" +
                   $"Title: {result.Title}\n" +
                   $"State: {result.State}\n" +
                   $"URL: {result.Url}\n" +
                   $"Updated: {result.UpdatedOn}";
        }
        catch (HttpRequestException ex)
        {
            return $"❌ Failed to update pull request: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"❌ Error: {ex.Message}";
        }
    }
}
