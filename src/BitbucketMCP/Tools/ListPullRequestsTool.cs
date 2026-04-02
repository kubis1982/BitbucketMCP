using BitbucketMCP.Configuration;
using BitbucketMCP.Generated;
using BitbucketMCP.Generated.Repositories.Item.Item.Pullrequests;
using BitbucketMCP.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class ListPullRequestsTool(BitbucketApiClient client, BitbucketConfig config)
{
    [McpServerTool(Name = "list_pull_requests")]
    [Description("Lists pull requests in a Bitbucket repository with optional filtering")]
    public async Task<List<PullResponse>> ListPullRequests(
        [Description("The repository slug (e.g., 'myrepo')")] string repo,
        [Description("Filter by state: OPEN, MERGED, DECLINED, SUPERSEDED (optional, defaults to OPEN)")] string? state = null)
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

        return result == null || result.Values == null ? [] : result.Values.Where(p => p != null).Select(p => PullResponse.From(p!)).ToList();
    }
}
