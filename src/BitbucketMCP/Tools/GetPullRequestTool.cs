using BitbucketMCP.Configuration;
using BitbucketMCP.Generated;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class GetPullRequestTool(BitbucketApiClient client, BitbucketConfig config)
{
    [McpServerTool(Name = "get_pull_request")]
    [Description("Retrieves details of a specific pull request from a Bitbucket repository")]
    public async Task<string> GetPullRequest(
        [Description("The repository slug (e.g., 'myrepo')")] string repo,
        [Description("The pull request ID number")] int prId)
    {
        try
        {
            var result = await client.Repositories[config.Workspace][repo].Pullrequests[prId].GetAsync();

            if (result == null)
                return $"❌ Pull request {prId} not found";

            return $"📋 Pull Request Details\n\n" +
                   $"ID: {result.Id}\n" +
                   $"Title: {result.Title}\n" +
                   $"Description: {result.Summary?.Raw ?? "N/A"}\n" +
                   $"State: {result.State}\n" +
                   $"Comments: {result.CommentCount ?? 0}\n" +
                   $"Tasks: {result.TaskCount ?? 0}\n" +
                   $"Created: {result.CreatedOn?.ToString("O") ?? "N/A"}\n" +
                   $"Updated: {result.UpdatedOn?.ToString("O") ?? "N/A"}\n" +
                   (result.MergeCommit?.Hash != null ? $"Merge Commit: {result.MergeCommit.Hash}\n" : "") +
                   $"URL: {result.Links?.Html?.Href ?? "N/A"}";
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
