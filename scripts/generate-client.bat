@echo off
REM Bitbucket API Client Generator
REM Wrapper script to execute PowerShell generation script

setlocal

REM Get the directory where this batch file is located
set "SCRIPT_DIR=%~dp0"

REM Execute PowerShell script with proper execution policy
pwsh -ExecutionPolicy Bypass -NoProfile -File "%SCRIPT_DIR%generate-client.ps1" %*

REM Propagate exit code
exit /b %ERRORLEVEL%
