#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates Bitbucket API client using Microsoft Kiota from OpenAPI specification.

.DESCRIPTION
    This script automates the generation of a strongly-typed Bitbucket API client.
    It downloads the latest OpenAPI spec from Bitbucket, validates it, and generates
    C# client code using Kiota.

.EXAMPLE
    .\generate-client.ps1
    Generates the client with default settings.

.NOTES
    Requires: Microsoft.OpenApi.Kiota dotnet tool (installed locally)
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Configuration
$SwaggerUrl = "https://dac-static.atlassian.com/cloud/bitbucket/swagger.v3.json?_v=2.300.161"
$OpenApiDir = "openapi"
$OpenApiFile = Join-Path $OpenApiDir "bitbucket-swagger.json"
$OutputDir = "src\BitbucketMCP\Generated"
$Namespace = "BitbucketMCP.Generated"

# Navigate to repository root
$ScriptDir = Split-Path -Parent $PSCommandPath
$RepoRoot = Split-Path -Parent $ScriptDir
Set-Location $RepoRoot

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Bitbucket API Client Generator (Kiota)" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Restore dotnet tools
Write-Host "[1/4] Restoring dotnet tools..." -ForegroundColor Yellow
try {
    dotnet tool restore 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restore dotnet tools"
    }
    Write-Host "✓ Tools restored successfully" -ForegroundColor Green
} catch {
    Write-Error "Failed to restore dotnet tools: $_"
    exit 1
}

# Step 2: Download OpenAPI specification
Write-Host ""
Write-Host "[2/4] Downloading OpenAPI specification..." -ForegroundColor Yellow
Write-Host "  Source: $SwaggerUrl" -ForegroundColor Gray

try {
    # Create openapi directory if it doesn't exist
    if (-not (Test-Path $OpenApiDir)) {
        New-Item -ItemType Directory -Path $OpenApiDir -Force | Out-Null
    }

    # Download the spec
    $ProgressPreference = 'SilentlyContinue'
    Invoke-WebRequest -Uri $SwaggerUrl -OutFile $OpenApiFile -UseBasicParsing
    $ProgressPreference = 'Continue'

    # Validate JSON
    $null = Get-Content $OpenApiFile -Raw | ConvertFrom-Json
    
    $FileSize = (Get-Item $OpenApiFile).Length
    Write-Host "✓ Downloaded and validated ($([math]::Round($FileSize/1KB, 2)) KB)" -ForegroundColor Green
} catch {
    Write-Error "Failed to download OpenAPI spec: $_"
    exit 1
}

# Step 3: Clean output directory
Write-Host ""
Write-Host "[3/4] Preparing output directory..." -ForegroundColor Yellow

if (Test-Path $OutputDir) {
    Write-Host "  Cleaning existing generated code..." -ForegroundColor Gray
    Remove-Item -Path $OutputDir -Recurse -Force
}

Write-Host "✓ Output directory ready: $OutputDir" -ForegroundColor Green

# Step 4: Generate client with Kiota
Write-Host ""
Write-Host "[4/4] Generating Bitbucket API client..." -ForegroundColor Yellow
Write-Host "  Language: C#" -ForegroundColor Gray
Write-Host "  Namespace: $Namespace" -ForegroundColor Gray

try {
    $kiotaArgs = @(
        "generate"
        "--language", "CSharp"
        "--openapi", $OpenApiFile
        "--output", $OutputDir
        "--namespace-name", $Namespace
        "--class-name", "BitbucketApiClient"
        "--clean-output"
        "--clear-cache"
    )

    Write-Host ""
    Write-Host "  Executing: dotnet kiota $($kiotaArgs -join ' ')" -ForegroundColor Gray
    Write-Host ""

    & dotnet kiota @kiotaArgs

    if ($LASTEXITCODE -ne 0) {
        throw "Kiota generation failed with exit code $LASTEXITCODE"
    }

    # Count generated files
    $GeneratedFiles = Get-ChildItem -Path $OutputDir -Recurse -File | Measure-Object
    Write-Host ""
    Write-Host "✓ Client generated successfully ($($GeneratedFiles.Count) files)" -ForegroundColor Green

} catch {
    Write-Error "Failed to generate client: $_"
    exit 1
}

# Summary
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Generation Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Generated files location: $OutputDir" -ForegroundColor White
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Review generated code in $OutputDir" -ForegroundColor Gray
Write-Host "  2. Add required NuGet packages to the project" -ForegroundColor Gray
Write-Host "  3. Update BitbucketApiClient to use generated client" -ForegroundColor Gray
Write-Host ""
