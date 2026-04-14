# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY src/BitbucketMCP/*.csproj ./BitbucketMCP/
RUN dotnet restore "./BitbucketMCP/BitbucketMCP.csproj"

# Copy source code and build
COPY src/BitbucketMCP/ ./BitbucketMCP/
WORKDIR "/src/BitbucketMCP"
RUN dotnet build "BitbucketMCP.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BitbucketMCP.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Expose HTTP port for MCP Server
EXPOSE 8080

# Create non-root user
RUN groupadd -r mcpuser && useradd -r -g mcpuser mcpuser && chown -R mcpuser:mcpuser /app
USER mcpuser

# Copy published app
COPY --from=publish /app/publish .

# Configure ASP.NET Core to listen on 8080
ENV ASPNETCORE_URLS=http://+:8080

# Set entry point
ENTRYPOINT ["dotnet", "Kubis1982.Atlassian.Bitbucket.MCP.dll", "--transport=http"]
