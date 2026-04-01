using BitbucketMCP.Configuration;
using BitbucketMCP.Models;
using KiotaClient = BitbucketMCP.Generated.BitbucketApiClient;
using KiotaModels = BitbucketMCP.Generated.Models;

namespace BitbucketMCP.Services;

/// <summary>
/// Bitbucket API client using Kiota-generated client
/// </summary>
public class BitbucketApiClient
{
    private readonly KiotaClient _client;

    public BitbucketApiClient(KiotaClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<PullRequestResponse> CreatePullRequest(
        string workspace,
        string repo,
        PullRequestRequest request)
    {
        var prRequest = new KiotaModels.Pullrequest
        {
            Title = request.Title,
            Summary = new KiotaModels.Pullrequest_summary
            {
                Raw = request.Description ?? string.Empty
            },
            Source = new KiotaModels.Pullrequest_endpoint
            {
                Branch = new KiotaModels.Pullrequest_endpoint_branch
                {
                    Name = request.SourceBranch
                }
            },
            Destination = new KiotaModels.Pullrequest_endpoint
            {
                Branch = new KiotaModels.Pullrequest_endpoint_branch
                {
                    Name = request.DestinationBranch
                }
            },
            CloseSourceBranch = false,
            Draft = request.IsDraft
        };

        // Add reviewers if specified
        if (request.Reviewers?.Any() == true)
        {
            prRequest.Reviewers = request.Reviewers.Select(uuid => new KiotaModels.Account
            {
                Uuid = uuid
            }).ToList();
        }

        var result = await _client.Repositories[workspace][repo].Pullrequests.PostAsync(prRequest);
        
        return MapToResponse(result);
    }

    public async Task<PullRequestResponse> UpdatePullRequest(
        string workspace,
        string repo,
        int prId,
        UpdatePullRequestRequest request)
    {
        // First get the current PR to preserve fields we're not updating
        var current = await _client.Repositories[workspace][repo].Pullrequests[prId].GetAsync();
        
        if (current == null)
            throw new InvalidOperationException($"Pull request {prId} not found");

        // Update only specified fields
        if (!string.IsNullOrWhiteSpace(request.Title))
            current.Title = request.Title;

        if (request.Description is not null)
        {
            current.Summary = new KiotaModels.Pullrequest_summary
            {
                Raw = request.Description
            };
        }

        if (request.Reviewers is not null)
        {
            current.Reviewers = request.Reviewers.Select(uuid => new KiotaModels.Account
            {
                Uuid = uuid
            }).ToList();
        }

        if (request.IsDraft.HasValue)
            current.Draft = request.IsDraft;

        var result = await _client.Repositories[workspace][repo].Pullrequests[prId].PutAsync(current);
        
        return MapToResponse(result);
    }

    public async Task<PullRequestResponse> GetPullRequest(
        string workspace,
        string repo,
        int prId)
    {
        var result = await _client.Repositories[workspace][repo].Pullrequests[prId].GetAsync();
        
        return MapToResponse(result);
    }

    private PullRequestResponse MapToResponse(KiotaModels.Pullrequest? pr)
    {
        if (pr == null)
            throw new InvalidOperationException("Pull request response is null");

        return new PullRequestResponse
        {
            Id = pr.Id ?? 0,
            Title = pr.Title ?? string.Empty,
            Description = pr.Summary?.Raw ?? string.Empty,
            State = pr.State?.ToString() ?? string.Empty,
            CreatedOn = pr.CreatedOn?.ToString("O") ?? string.Empty,
            UpdatedOn = pr.UpdatedOn?.ToString("O") ?? string.Empty,
            MergeCommit = pr.MergeCommit?.Hash,
            CommentCount = pr.CommentCount ?? 0,
            TaskCount = pr.TaskCount ?? 0,
            Url = pr.Links?.Html?.Href ?? string.Empty
        };
    }
}
