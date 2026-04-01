using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BitbucketMCP.Configuration;
using BitbucketMCP.Models;

namespace BitbucketMCP.Services;

public class BitbucketApiClient
{
    private readonly HttpClient _httpClient;
    private readonly BitbucketConfig _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public BitbucketApiClient(HttpClient httpClient, BitbucketConfig config)
    {
        _httpClient = httpClient;
        _config = config;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };

        ConfigureAuthentication();
    }

    private void ConfigureAuthentication()
    {
        if (_config.AuthType == "app_password")
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_config.Username}:{_config.AppPassword}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
        }
        else if (_config.AuthType == "oauth_token")
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config.Token);
        }
    }

    public async Task<PullRequestResponse> CreatePullRequest(
        string workspace,
        string repo,
        PullRequestRequest request)
    {
        var payload = new
        {
            title = request.Title,
            description = request.Description ?? string.Empty,
            source = new { branch = new { name = request.SourceBranch } },
            destination = new { branch = new { name = request.DestinationBranch } },
            reviewers = request.Reviewers?.Select(r => new { uuid = r }).ToArray() ?? Array.Empty<object>(),
            close_source_branch = false
        };

        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"/repositories/{workspace}/{repo}/pullrequests",
            content);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return MapToPullRequestResponse(responseJson);
    }

    public async Task<PullRequestResponse> UpdatePullRequest(
        string workspace,
        string repo,
        int prId,
        UpdatePullRequestRequest request)
    {
        var payload = new Dictionary<string, object?>();

        if (!string.IsNullOrWhiteSpace(request.Title))
            payload["title"] = request.Title;

        if (request.Description is not null)
            payload["description"] = request.Description;

        if (request.Reviewers is not null)
            payload["reviewers"] = request.Reviewers.Select(r => new { uuid = r }).ToArray();

        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(
            $"/repositories/{workspace}/{repo}/pullrequests/{prId}",
            content);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return MapToPullRequestResponse(responseJson);
    }

    public async Task<PullRequestResponse> GetPullRequest(
        string workspace,
        string repo,
        int prId)
    {
        var response = await _httpClient.GetAsync(
            $"/repositories/{workspace}/{repo}/pullrequests/{prId}");

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return MapToPullRequestResponse(responseJson);
    }

    private PullRequestResponse MapToPullRequestResponse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return new PullRequestResponse
        {
            Id = root.GetProperty("id").GetInt32(),
            Title = root.GetProperty("title").GetString() ?? string.Empty,
            Description = root.TryGetProperty("description", out var desc) ? desc.GetString() ?? string.Empty : string.Empty,
            State = root.GetProperty("state").GetString() ?? string.Empty,
            CreatedOn = root.GetProperty("created_on").GetString() ?? string.Empty,
            UpdatedOn = root.GetProperty("updated_on").GetString() ?? string.Empty,
            MergeCommit = root.TryGetProperty("merge_commit", out var mergeCommit) && mergeCommit.ValueKind != JsonValueKind.Null
                ? mergeCommit.GetProperty("hash").GetString()
                : null,
            CommentCount = root.TryGetProperty("comment_count", out var commentCount) ? commentCount.GetInt32() : 0,
            TaskCount = root.TryGetProperty("task_count", out var taskCount) ? taskCount.GetInt32() : 0,
            Url = root.TryGetProperty("links", out var links) && links.TryGetProperty("html", out var html)
                ? html.GetProperty("href").GetString() ?? string.Empty
                : string.Empty
        };
    }
}
