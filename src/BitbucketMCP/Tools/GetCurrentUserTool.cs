using BitbucketMCP.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace BitbucketMCP.Tools;

[McpServerToolType]
public class GetCurrentUserTool(BitbucketRestClient client)
{
    [McpServerTool(Name = "get_current_user")]
    [Description("Retrieves the Bitbucket account currently authenticated with the configured credentials")]
    public async Task<UserResponse> GetCurrentUser()
    {
        try
        {
            var result = await client.User.GetAsync() ?? throw new McpException("Failed to retrieve current user: No response from API");

            return UserResponse.From(result);
        }
        catch (Error ex)
        {
            var detail = ex.ErrorProp?.Detail ?? ex.ErrorProp?.Message ?? ex.Message;
            throw new McpException($"Bitbucket API rejected the request: {detail}", ex);
        }
    }
}
