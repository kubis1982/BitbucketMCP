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
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

# Create non-root user
RUN useradd -m -u 1000 mcpuser && chown -R mcpuser:mcpuser /app
USER mcpuser

# Copy published app
COPY --from=publish /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "BitbucketMCP.dll"]
