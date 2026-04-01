using System.ComponentModel;
using BitbucketMCP.Services;
using ModelContextProtocol.Server;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class GetPullRequestTool(BitbucketApiClient apiClient)
{
    [McpServerTool("get_pull_request", "Retrieves details of a specific pull request from a Bitbucket repository")]
    public async Task<string> GetPullRequest(
        [Description("The workspace slug (e.g., 'myworkspace')")] string workspace,
        [Description("The repository slug (e.g., 'myrepo')")] string repo,
        [Description("The pull request ID number")] int prId)
    {
        try
        {
            var result = await apiClient.GetPullRequest(workspace, repo, prId);

            return $"📋 Pull Request Details\n\n" +
                   $"ID: {result.Id}\n" +
                   $"Title: {result.Title}\n" +
                   $"Description: {result.Description}\n" +
                   $"State: {result.State}\n" +
                   $"Comments: {result.CommentCount}\n" +
                   $"Tasks: {result.TaskCount}\n" +
                   $"Created: {result.CreatedOn}\n" +
                   $"Updated: {result.UpdatedOn}\n" +
                   (result.MergeCommit != null ? $"Merge Commit: {result.MergeCommit}\n" : "") +
                   $"URL: {result.Url}";
        }
        catch (HttpRequestException ex)
        {
            return $"❌ Failed to retrieve pull request: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"❌ Error: {ex.Message}";
        }
    }
}
