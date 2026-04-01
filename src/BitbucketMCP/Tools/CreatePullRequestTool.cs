using System.ComponentModel;
using ModelContextProtocol.Server;
using KiotaClient = BitbucketMCP.Generated.BitbucketApiClient;
using KiotaModels = BitbucketMCP.Generated.Models;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class CreatePullRequestTool(KiotaClient client)
{
    [McpServerTool(Name = "create_pull_request")]
    [Description("Creates a new pull request in a Bitbucket repository")]
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
            var pr = new KiotaModels.Pullrequest
            {
                Title = title,
                Summary = new KiotaModels.Pullrequest_summary
                {
                    Raw = isDraft && !string.IsNullOrEmpty(description) 
                        ? $"[DRAFT] {description}" 
                        : description ?? string.Empty
                },
                Source = new KiotaModels.Pullrequest_endpoint
                {
                    Branch = new KiotaModels.Pullrequest_endpoint_branch
                    {
                        Name = sourceBranch
                    }
                },
                Destination = new KiotaModels.Pullrequest_endpoint
                {
                    Branch = new KiotaModels.Pullrequest_endpoint_branch
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
                pr.Reviewers = reviewers.Select(uuid => new KiotaModels.Account
                {
                    Uuid = uuid
                }).ToList();
            }

            var result = await client.Repositories[workspace][repo].Pullrequests.PostAsync(pr);

            if (result == null)
                return "❌ Failed to create pull request: No response from API";

            return $"✅ Pull request created successfully!\n\n" +
                   $"ID: {result.Id}\n" +
                   $"Title: {result.Title}\n" +
                   $"State: {result.State}\n" +
                   $"URL: {result.Links?.Html?.Href ?? "N/A"}\n" +
                   $"Created: {result.CreatedOn?.ToString("O") ?? "N/A"}";
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
