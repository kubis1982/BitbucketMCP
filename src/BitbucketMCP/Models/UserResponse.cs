namespace BitbucketMCP.Models
{
    public class UserResponse
    {
        public string? Uuid { get; set; }
        public string? DisplayName { get; set; }
        public string? AccountType { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }
        public string? AvatarUrl { get; set; }

        public static UserResponse From(Account account)
        {
            if (account == null) return null!;

            return new UserResponse
            {
                Uuid = account.Uuid,
                DisplayName = account.DisplayName,
                AccountType = account.Type,
                CreatedOn = account.CreatedOn,
                AvatarUrl = account.Links?.Avatar?.Href
            };
        }
    }
}
