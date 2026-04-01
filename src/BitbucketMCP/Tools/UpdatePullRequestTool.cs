using System.ComponentModel;
using ModelContextProtocol.Server;
using KiotaClient = BitbucketMCP.Generated.BitbucketApiClient;
using KiotaModels = BitbucketMCP.Generated.Models;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class UpdatePullRequestTool(KiotaClient client)
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
            // GET current PR
            var current = await client.Repositories[workspace][repo].Pullrequests[prId].GetAsync();

            if (current == null)
                return $"❌ Pull request {prId} not found";

            // Modify only specified fields
            if (!string.IsNullOrWhiteSpace(title))
                current.Title = title;

            if (description is not null)
            {
                current.Summary = new KiotaModels.Pullrequest_summary
                {
                    Raw = description
                };
            }

            if (reviewers is not null)
            {
                current.Reviewers = reviewers.Select(uuid => new KiotaModels.Account
                {
                    Uuid = uuid
                }).ToList();
            }

            if (isDraft.HasValue)
                current.Draft = isDraft;

            // PUT updated PR
            var result = await client.Repositories[workspace][repo].Pullrequests[prId].PutAsync(current);

            if (result == null)
                return "❌ Failed to update pull request: No response from API";

            return $"✅ Pull request updated successfully!\n\n" +
                   $"ID: {result.Id}\n" +
                   $"Title: {result.Title}\n" +
                   $"State: {result.State}\n" +
                   $"URL: {result.Links?.Html?.Href ?? "N/A"}\n" +
                   $"Updated: {result.UpdatedOn?.ToString("O") ?? "N/A"}";
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
