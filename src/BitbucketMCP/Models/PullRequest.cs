namespace BitbucketMCP.Models;

public class PullRequestRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string SourceBranch { get; set; }
    public required string DestinationBranch { get; set; }
    public List<string>? Reviewers { get; set; }
    public bool IsDraft { get; set; }
}

public class PullRequestResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string CreatedOn { get; set; } = string.Empty;
    public string UpdatedOn { get; set; } = string.Empty;
    public string? MergeCommit { get; set; }
    public int CommentCount { get; set; }
    public int TaskCount { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class UpdatePullRequestRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Reviewers { get; set; }
    public bool? IsDraft { get; set; }
}
