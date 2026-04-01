# Bitbucket MCP Server - API Documentation

## Overview

The Bitbucket MCP Server exposes MCP tools for managing Pull Requests in Bitbucket Cloud repositories. This document provides detailed API specifications, request/response examples, and error handling information.

## Base Configuration

### Environment Variables

All configuration is done through environment variables:

```bash
BITBUCKET_AUTH_TYPE=app_password|oauth_token
BITBUCKET_USERNAME=your-username  # Required for app_password
BITBUCKET_APP_PASSWORD=your-app-password  # Required for app_password
BITBUCKET_TOKEN=your-bearer-token  # Required for oauth_token
BITBUCKET_WORKSPACE=default-workspace  # Optional
LOG_LEVEL=Information  # Optional: Trace, Debug, Information, Warning, Error
```

## MCP Tools

### 1. CreatePullRequest

Creates a new pull request in a Bitbucket repository.

#### Tool Name
`CreatePullRequest`

#### Description
Creates a new pull request in a Bitbucket repository

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `workspace` | string | Yes | The workspace slug (e.g., 'myworkspace') |
| `repo` | string | Yes | The repository slug (e.g., 'myrepo') |
| `title` | string | Yes | The title of the pull request |
| `description` | string | No | The description/body of the pull request |
| `sourceBranch` | string | Yes | The source branch name (the branch to merge from) |
| `destinationBranch` | string | Yes | The destination branch name (the branch to merge into, usually 'main' or 'master') |
| `reviewers` | List<string> | No | Optional array of reviewer account UUIDs in format '{account-id}' |
| `isDraft` | boolean | No | Whether this is a draft pull request (default: false, managed via description prefix) |

#### Example Request

```json
{
  "workspace": "myworkspace",
  "repo": "myrepo",
  "title": "Feature: Add user authentication",
  "description": "This PR implements OAuth 2.0 authentication for the application.\n\n## Changes\n- Added OAuth provider\n- Updated login flow\n- Added tests",
  "sourceBranch": "feature/authentication",
  "destinationBranch": "main",
  "reviewers": ["{1234abcd-5678-efgh-9012-ijkl3456mnop}"],
  "isDraft": false
}
```

#### Example Success Response

```
✅ Pull Request Created Successfully!

📋 Details:
• ID: 42
• Title: Feature: Add user authentication
• State: OPEN
• URL: https://bitbucket.org/myworkspace/myrepo/pull-requests/42

📅 Timestamps:
• Created: 2026-04-01T14:30:00.000Z
• Updated: 2026-04-01T14:30:00.000Z

💬 Comments: 0
✓ Tasks: 0
```

#### Bitbucket API Mapping

- **HTTP Method**: POST
- **Endpoint**: `/repositories/{workspace}/{repo}/pullrequests`
- **Base URL**: https://api.bitbucket.org/2.0

#### Request Payload to Bitbucket API

```json
{
  "title": "Feature: Add user authentication",
  "description": "This PR implements OAuth 2.0 authentication...",
  "source": {
    "branch": {
      "name": "feature/authentication"
    }
  },
  "destination": {
    "branch": {
      "name": "main"
    }
  },
  "reviewers": [
    {"uuid": "{1234abcd-5678-efgh-9012-ijkl3456mnop}"}
  ],
  "close_source_branch": false
}
```

---

### 2. UpdatePullRequest

Updates an existing pull request in a Bitbucket repository.

#### Tool Name
`UpdatePullRequest`

#### Description
Updates an existing pull request in a Bitbucket repository

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `workspace` | string | Yes | The workspace slug (e.g., 'myworkspace') |
| `repo` | string | Yes | The repository slug (e.g., 'myrepo') |
| `prId` | int | Yes | The pull request ID number |
| `title` | string | No | The new title for the pull request (optional) |
| `description` | string | No | The new description/body for the pull request (optional) |
| `reviewers` | List<string> | No | Updated array of reviewer account UUIDs in format '{account-id}' (optional) |
| `isDraft` | boolean | No | Update draft status (optional, managed via description prefix) |

#### Example Request

```json
{
  "workspace": "myworkspace",
  "repo": "myrepo",
  "prId": 42,
  "title": "Feature: Add user authentication (Updated)",
  "description": "Updated description with additional context",
  "reviewers": [
    "{1234abcd-5678-efgh-9012-ijkl3456mnop}",
    "{9876zyxw-5432-vuts-1098-rqpo7654nmlk}"
  ]
}
```

#### Example Success Response

```
✅ Pull Request Updated Successfully!

📋 Updated Details:
• ID: 42
• Title: Feature: Add user authentication (Updated)
• State: OPEN
• URL: https://bitbucket.org/myworkspace/myrepo/pull-requests/42

📅 Timestamps:
• Created: 2026-04-01T14:30:00.000Z
• Updated: 2026-04-01T15:45:00.000Z

💬 Comments: 3
✓ Tasks: 1
```

#### Bitbucket API Mapping

- **HTTP Method**: PUT
- **Endpoint**: `/repositories/{workspace}/{repo}/pullrequests/{pr_id}`
- **Base URL**: https://api.bitbucket.org/2.0

#### Request Payload to Bitbucket API

```json
{
  "title": "Feature: Add user authentication (Updated)",
  "description": "Updated description with additional context",
  "reviewers": [
    {"uuid": "{1234abcd-5678-efgh-9012-ijkl3456mnop}"},
    {"uuid": "{9876zyxw-5432-vuts-1098-rqpo7654nmlk}"}
  ]
}
```

---

### 3. GetPullRequest

Retrieves details of a specific pull request from a Bitbucket repository.

#### Tool Name
`GetPullRequest`

#### Description
Retrieves details of a specific pull request from a Bitbucket repository

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `workspace` | string | Yes | The workspace slug (e.g., 'myworkspace') |
| `repo` | string | Yes | The repository slug (e.g., 'myrepo') |
| `prId` | int | Yes | The pull request ID number |

#### Example Request

```json
{
  "workspace": "myworkspace",
  "repo": "myrepo",
  "prId": 42
}
```

#### Example Success Response

```
📋 Pull Request Details

• ID: 42
• Title: Feature: Add user authentication
• State: OPEN
• URL: https://bitbucket.org/myworkspace/myrepo/pull-requests/42

📅 Timestamps:
• Created: 2026-04-01T14:30:00.000Z
• Updated: 2026-04-01T15:45:00.000Z

💬 Comments: 3
✓ Tasks: 1
```

#### Bitbucket API Mapping

- **HTTP Method**: GET
- **Endpoint**: `/repositories/{workspace}/{repo}/pullrequests/{pr_id}`
- **Base URL**: https://api.bitbucket.org/2.0

---

## Error Handling

All tools implement comprehensive error handling and return meaningful error messages.

### Common Error Scenarios

#### 1. Authentication Failure (401 Unauthorized)

**Response:**
```
❌ Error creating pull request:
Authentication failed. Please check your credentials.

For app_password: Verify BITBUCKET_USERNAME and BITBUCKET_APP_PASSWORD
For oauth_token: Verify BITBUCKET_TOKEN is valid and not expired
```

**Causes:**
- Invalid username or app password
- Expired or invalid OAuth token
- Incorrect authentication type configured

#### 2. Permission Denied (403 Forbidden)

**Response:**
```
❌ Error creating pull request:
Permission denied. Please ensure your credentials have write access to this repository.
```

**Causes:**
- Account lacks write permissions to the repository
- App password doesn't have Pull Request write permission
- OAuth token lacks required scopes

#### 3. Resource Not Found (404 Not Found)

**Response:**
```
❌ Error getting pull request:
Pull request not found. Please verify the workspace, repository, and PR ID are correct.
```

**Causes:**
- Workspace doesn't exist
- Repository doesn't exist
- Pull request ID is invalid
- Wrong workspace/repository specified

#### 4. Rate Limiting (429 Too Many Requests)

**Response:**
```
❌ Error updating pull request:
Rate limit exceeded. Please wait before trying again.
```

**Causes:**
- Too many API requests in a short time
- Bitbucket API rate limits reached

#### 5. Server Error (500+ status codes)

**Response:**
```
❌ Error creating pull request:
Bitbucket API server error. Please try again later.
```

**Causes:**
- Bitbucket API temporary issues
- Service degradation

#### 6. Validation Errors

**Response:**
```
❌ Error creating pull request:
Invalid request: Source branch 'invalid-branch' does not exist in repository
```

**Causes:**
- Branch doesn't exist
- Invalid parameters
- Malformed requests

---

## Response Models

### PullRequestResponse

```csharp
public class PullRequestResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string State { get; set; }  // "OPEN", "MERGED", "DECLINED", "SUPERSEDED"
    public string CreatedOn { get; set; }  // ISO 8601 timestamp
    public string UpdatedOn { get; set; }  // ISO 8601 timestamp
    public string? MergeCommit { get; set; }
    public int CommentCount { get; set; }
    public int TaskCount { get; set; }
    public string Url { get; set; }  // Direct link to PR
}
```

---

## Authentication Details

### App Password Authentication

1. Generate an app password in Bitbucket
2. Set required environment variables:
   ```bash
   BITBUCKET_AUTH_TYPE=app_password
   BITBUCKET_USERNAME=your-username
   BITBUCKET_APP_PASSWORD=generated-password
   ```
3. API uses HTTP Basic Authentication
4. Header format: `Authorization: Basic base64(username:app_password)`

**Required App Password Permissions:**
- Pull requests: Read, Write
- Repositories: Read

### OAuth Token Authentication

1. Obtain OAuth 2.0 bearer token
2. Set required environment variables:
   ```bash
   BITBUCKET_AUTH_TYPE=oauth_token
   BITBUCKET_TOKEN=your-bearer-token
   ```
3. API uses Bearer token authentication
4. Header format: `Authorization: Bearer your-token`

**Required OAuth Scopes:**
- `pullrequest:write`
- `repository:read`

---

## Testing

### Manual Testing with MCP Client

1. Start the server:
   ```bash
   dotnet run --project src/BitbucketMCP
   ```

2. Connect an MCP client (e.g., Claude Desktop, VS Code Copilot)

3. Test tool discovery:
   - Client should list: CreatePullRequest, UpdatePullRequest, GetPullRequest

4. Test PR creation:
   ```
   Use the CreatePullRequest tool with test data
   ```

5. Verify response and check Bitbucket for created PR

### Testing with Docker

1. Build image:
   ```bash
   docker build -t bitbucket-mcp:test .
   ```

2. Run with test credentials:
   ```bash
   docker run -it --rm \
     -e BITBUCKET_AUTH_TYPE=app_password \
     -e BITBUCKET_USERNAME=test-user \
     -e BITBUCKET_APP_PASSWORD=test-password \
     bitbucket-mcp:test
   ```

3. Connect MCP client to container

---

## Rate Limits

Bitbucket Cloud API rate limits:
- **Standard**: 1000 requests/hour per user
- **App passwords**: Share user's rate limit
- **OAuth**: Separate rate limit per app

**Best Practices:**
- Implement exponential backoff for 429 responses
- Cache PR data when possible
- Batch operations when feasible

---

## Troubleshooting

### Enable Debug Logging

```bash
LOG_LEVEL=Debug dotnet run --project src/BitbucketMCP
```

Logs are written to stderr to avoid interfering with MCP stdio communication.

### Check API Connectivity

```bash
curl -u username:app_password https://api.bitbucket.org/2.0/user
```

### Verify Configuration

The server validates configuration on startup and will exit with clear error messages if misconfigured.

---

## Version History

### v1.0.0 (2026-04-01)
- Initial release
- CreatePullRequest tool
- UpdatePullRequest tool
- GetPullRequest tool
- App password authentication
- OAuth token authentication
- Docker support
- Comprehensive error handling

---

## Support

For issues or questions:
- GitHub Issues: https://github.com/kubis1982/BitbucketMCP/issues
- Bitbucket API Docs: https://developer.atlassian.com/cloud/bitbucket/rest/
- MCP Protocol: https://modelcontextprotocol.io/
