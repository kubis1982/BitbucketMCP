using System.ComponentModel;
using BitbucketMCP.Services;
using ModelContextProtocol.Server;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class ListPullRequestsTool(BitbucketApiClient apiClient)
{
    [McpServerTool(Name = "list_pull_requests")]
    [Description("Lists pull requests in a Bitbucket repository with optional filtering")]
    public async Task<string> ListPullRequests(
        [Description("The workspace slug (e.g., 'myworkspace')")] string workspace,
        [Description("The repository slug (e.g., 'myrepo')")] string repo,
        [Description("Filter by state: OPEN, MERGED, DECLINED, SUPERSEDED (optional, defaults to OPEN)")] string? state = null)
    {
        try
        {
            var result = await apiClient.ListPullRequests(workspace, repo, state);

            if (!result.Any())
            {
                return $"📋 No pull requests found" + (state != null ? $" with state: {state}" : "");
            }

            var output = $"📋 Pull Requests in {workspace}/{repo}" + (state != null ? $" (State: {state})" : "") + $"\n\n";
            output += $"Total: {result.Count}\n\n";

            foreach (var pr in result)
            {
                output += $"PR #{pr.Id}: {pr.Title}\n";
                output += $"  State: {pr.State}\n";
                output += $"  Created: {pr.CreatedOn}\n";
                output += $"  Comments: {pr.CommentCount}, Tasks: {pr.TaskCount}\n";
                output += $"  URL: {pr.Url}\n\n";
            }

            return output;
        }
        catch (HttpRequestException ex)
        {
            return $"❌ Failed to list pull requests: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"❌ Error: {ex.Message}";
        }
    }
}
