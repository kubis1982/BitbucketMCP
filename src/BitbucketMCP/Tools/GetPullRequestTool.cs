using BitbucketMCP.Configuration;
using BitbucketMCP.Generated;
using BitbucketMCP.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class GetPullRequestTool(BitbucketApiClient client, BitbucketConfig config)
{
    [McpServerTool(Name = "get_pull_request")]
    [Description("Retrieves details of a specific pull request from a Bitbucket repository")]
    public async Task<PullResponse> GetPullRequest(
        [Description("The repository slug (e.g., 'myrepo')")] string repo,
        [Description("The pull request ID number")] int prId)
    {
        var result = await client.Repositories[config.Workspace][repo].Pullrequests[prId].GetAsync();

        if (result == null)
            throw new InvalidOperationException($"Pull request {prId} not found");

        return PullResponse.From(result);
    }
}
