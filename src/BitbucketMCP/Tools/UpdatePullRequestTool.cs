using BitbucketMCP.Configuration;
using BitbucketMCP.Generated;
using BitbucketMCP.Models;
using BitbucketMCP.Generated.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class UpdatePullRequestTool(BitbucketApiClient client, BitbucketConfig config)
{
    [McpServerTool(Name = "update_pull_request")]
    [Description("Updates an existing pull request in a Bitbucket repository")]
    public async Task<PullResponse> UpdatePullRequest(
        [Description("The repository slug (e.g., 'myrepo')")] string repo,
        [Description("The pull request ID number")] int prId,
        [Description("The new title for the pull request (optional)")] string? title = null,
        [Description("The new description/body for the pull request (optional)")] string? description = null,
        [Description("Updated array of reviewer account UUIDs in format '{account-id}' (optional)")] List<string>? reviewers = null,
        [Description("Update draft status (note: managed via description prefix)")] bool? isDraft = null)
    {
        // GET current PR
        var current = await client.Repositories[config.Workspace][repo].Pullrequests[prId].GetAsync();

        if (current == null)
            throw new InvalidOperationException($"Pull request {prId} not found");

        // Modify only specified fields
        if (!string.IsNullOrWhiteSpace(title))
            current.Title = title;

        if (description is not null)
        {
            current.Summary = new Pullrequest_summary
            {
                Raw = description
            };
        }

        if (reviewers is not null)
        {
            current.Reviewers = reviewers.Select(uuid => new Account
            {
                Uuid = uuid
            }).ToList();
        }

        if (isDraft.HasValue)
            current.Draft = isDraft;

        // PUT updated PR
        var result = await client.Repositories[config.Workspace][repo].Pullrequests[prId].PutAsync(current);

        if (result == null)
            throw new InvalidOperationException("Failed to update pull request: No response from API");

        return PullResponse.From(result);
    }
}
