using System;
using BitbucketMCP.Models;
using Xunit;

namespace BitbucketMCP.Tests
{
    public class UserResponseTests
    {
        [Fact]
        public void From_MapsFieldsCorrectly()
        {
            var account = new Account
            {
                Uuid = "{user-uuid}",
                DisplayName = "Jane Doe",
                Type = "user",
                CreatedOn = DateTimeOffset.Parse("2020-01-01T00:00:00Z"),
                Links = new Account_links
                {
                    Avatar = new Link { Href = "https://bitbucket.org/account/avatar.png" }
                }
            };

            var dto = UserResponse.From(account);

            Assert.Equal(account.Uuid, dto.Uuid);
            Assert.Equal(account.DisplayName, dto.DisplayName);
            Assert.Equal(account.Type, dto.AccountType);
            Assert.Equal(account.CreatedOn, dto.CreatedOn);
            Assert.Equal(account.Links.Avatar.Href, dto.AvatarUrl);
        }
    }
}
