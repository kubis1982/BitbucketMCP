using BitbucketMCP.Generated;
using BitbucketMCP.Generated.Repositories.Item.Item.Pullrequests;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class ListPullRequestsTool(BitbucketApiClient client, BitbucketConfig config)
{
    [McpServerTool(Name = "list_pull_requests")]
    [Description("Lists pull requests in a Bitbucket repository with optional filtering")]
    public async Task<string> ListPullRequests(
        [Description("The repository slug (e.g., 'myrepo')")] string repo,
        [Description("Filter by state: OPEN, MERGED, DECLINED, SUPERSEDED (optional, defaults to OPEN)")] string? state = null)
    {
        try
        {
            var result = await client.Repositories[config.Workspace][repo].Pullrequests.GetAsync(config =>
            {
                if (!string.IsNullOrEmpty(state))
                {
                    // Try to parse state to enum
                    if (Enum.TryParse<GetStateQueryParameterType>(state, true, out var stateEnum))
                    {
                        config.QueryParameters.StateAsGetStateQueryParameterType = stateEnum;
                    }
                }
            });

            if (result?.Values == null || !result.Values.Any())
            {
                return $"📋 No pull requests found" + (state != null ? $" with state: {state}" : "");
            }

            var output = $"📋 Pull Requests in {workspace}/{repo}" + (state != null ? $" (State: {state})" : "") + $"\n\n";
            output += $"Total: {result.Values.Count}\n\n";

            foreach (var pr in result.Values.Where(pr => pr != null))
            {
                output += $"PR #{pr!.Id}: {pr.Title}\n";
                output += $"  State: {pr.State}\n";
                output += $"  Created: {pr.CreatedOn?.ToString("O") ?? "N/A"}\n";
                output += $"  Comments: {pr.CommentCount ?? 0}, Tasks: {pr.TaskCount ?? 0}\n";
                output += $"  URL: {pr.Links?.Html?.Href ?? "N/A"}\n\n";
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
