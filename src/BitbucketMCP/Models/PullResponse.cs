using BitbucketMCP.Generated.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BitbucketMCP.Models
{
    public class PullResponse
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? State { get; set; }
        public string? Url { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
        public int? CommentCount { get; set; }
        public int? TaskCount { get; set; }
        public List<string>? Reviewers { get; set; }

        public static PullResponse From(Pullrequest pr)
        {
            if (pr == null) return null!;

            return new PullResponse
            {
                Id = pr.Id,
                Title = pr.Title,
                Description = pr.Summary?.Raw,
                State = pr.State?.ToString(),
                Url = pr.Links?.Html?.Href,
                CreatedOn = pr.CreatedOn,
                UpdatedOn = pr.UpdatedOn,
                CommentCount = pr.CommentCount,
                TaskCount = pr.TaskCount,
                Reviewers = pr.Reviewers?.Where(r => r != null)
                                       .Select(r => r!.Uuid ?? r!.DisplayName)
                                       .ToList()
            };
        }
    }
}