using Xunit;
using BitbucketMCP.Models;
using BitbucketMCP.Generated.Models;
using System;
using System.Collections.Generic;

namespace BitbucketMCP.Tests
{
    public class PullResponseTests
    {
        [Fact]
        public void From_MapsFieldsCorrectly()
        {
            var pr = new Pullrequest
            {
                Id = 42,
                Title = "Test PR",
                Summary = new Pullrequest_summary { Raw = "Desc" },
                State = BitbucketMCP.Generated.Models.Pullrequest_state.OPEN,
                Links = new Pullrequest_links { Html = new Pullrequest_links_html { Href = "http://example.com" } },
                CreatedOn = DateTimeOffset.Parse("2020-01-01T00:00:00Z"),
                UpdatedOn = DateTimeOffset.Parse("2020-01-02T00:00:00Z"),
                CommentCount = 3,
                TaskCount = 1,
                Reviewers = new List<Account> { new Account { Uuid = "{uuid}", DisplayName = "User" } }
            };

            var dto = PullResponse.From(pr);

            Assert.Equal(pr.Id, dto.Id);
            Assert.Equal(pr.Title, dto.Title);
            Assert.Equal(pr.Summary?.Raw, dto.Description);
            Assert.Equal(pr.State?.ToString(), dto.State);
            Assert.Equal(pr.Links?.Html?.Href, dto.Url);
            Assert.Equal(pr.CreatedOn, dto.CreatedOn);
            Assert.Equal(pr.UpdatedOn, dto.UpdatedOn);
            Assert.Equal(pr.CommentCount, dto.CommentCount);
            Assert.Equal(pr.TaskCount, dto.TaskCount);
            Assert.Contains("{uuid}", dto.Reviewers);
        }
    }
}
