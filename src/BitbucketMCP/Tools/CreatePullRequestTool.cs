using BitbucketMCP.Configuration;
using BitbucketMCP.Generated;
using BitbucketMCP.Generated.Models;
using BitbucketMCP.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Linq;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class CreatePullRequestTool(BitbucketApiClient client, BitbucketConfig config)
{
    [McpServerTool(Name = "create_pull_request")]
    [Description("Creates a new pull request in a Bitbucket repository")]
    public async Task<PullResponse> CreatePullRequest(
        [Description("The repository slug (e.g., 'myrepo')")] string repo,
        [Description("The title of the pull request")] string title,
        [Description("The description/body of the pull request")] string? description,
        [Description("The source branch name (the branch to merge from)")] string sourceBranch,
        [Description("The destination branch name (the branch to merge into, usually 'main' or 'master')")] string destinationBranch,
        [Description("Optional array of reviewer account UUIDs in format '{account-id}'")] List<string>? reviewers = null,
        [Description("Whether this is a draft pull request (note: managed via description prefix)")] bool isDraft = false)
    {
        var pr = new Pullrequest
        {
            Title = title,
            Summary = new Pullrequest_summary
            {
                Raw = isDraft && !string.IsNullOrEmpty(description)
                    ? $"[DRAFT] {description}"
                    : description ?? string.Empty
            },
            Source = new Pullrequest_endpoint
            {
                Branch = new Pullrequest_endpoint_branch
                {
                    Name = sourceBranch
                }
            },
            Destination = new Pullrequest_endpoint
            {
                Branch = new Pullrequest_endpoint_branch
                {
                    Name = destinationBranch
                }
            },
            CloseSourceBranch = false,
            Draft = isDraft
        };

        // Add reviewers if specified
        if (reviewers?.Any() == true)
        {
            pr.Reviewers = reviewers.Select(uuid => new Account
            {
                Uuid = uuid
            }).ToList();
        }

        var result = await client.Repositories[config.Workspace][repo].Pullrequests.PostAsync(pr);

        if (result == null)
            throw new InvalidOperationException("Failed to create pull request: No response from API");

        return PullResponse.From(result);
    }
}
