# TotalRecall release builder
# Publishes the WinForms app + MCP server as self-contained x64, bundles both
# into a single ZIP suitable for upload as a GitHub Release asset.
#
# Usage:
#   .\build-release.ps1                   # version = read from arg or default 1.0.0
#   .\build-release.ps1 -Version 1.2.3
#
# Requires: .NET 10 SDK, 7-zip or built-in Compress-Archive.

[CmdletBinding()]
param(
    [string]$Version = '1.0.0',
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$rid          = 'win-x64'
$stagingRoot  = Join-Path $root "artifacts\staging\TotalRecall-$Version-$rid"
$artifactsDir = Join-Path $root 'artifacts'
$zipPath      = Join-Path $root "TotalRecall-$Version-$rid.zip"

Write-Host "TotalRecall release builder" -ForegroundColor Cyan
Write-Host "  Version       : $Version"
Write-Host "  Configuration : $Configuration"
Write-Host "  Runtime       : $rid"
Write-Host "  Staging       : $stagingRoot"
Write-Host "  Output ZIP    : $zipPath"
Write-Host ""

if (Test-Path $stagingRoot) { Remove-Item $stagingRoot -Recurse -Force }
if (Test-Path $zipPath)     { Remove-Item $zipPath -Force }
New-Item -ItemType Directory -Path $stagingRoot | Out-Null

function Publish-Project {
    param(
        [string]$Project,
        [string]$OutSubdir,
        [switch]$NoSelfContained
    )
    $out = Join-Path $stagingRoot $OutSubdir
    Write-Host "Publishing $Project -> $out" -ForegroundColor Yellow
    $args = @(
        'publish', $Project,
        '-c', $Configuration,
        '-r', $rid,
        '--self-contained', $(if ($NoSelfContained) { 'false' } else { 'true' }),
        '-p:PublishSingleFile=false',
        '-p:DebugType=none',
        '-p:DebugSymbols=false',
        '-o', $out,
        '--nologo'
    )
    & dotnet @args | ForEach-Object {
        if ($_ -match '^\s*(error|Build FAILED)') { Write-Host $_ -ForegroundColor Red }
        elseif ($_ -match 'Build succeeded') { Write-Host $_ -ForegroundColor Green }
    }
    if ($LASTEXITCODE -ne 0) { throw "Publish failed for $Project (exit $LASTEXITCODE)" }
}

# --- 1. Publish both projects --------------------------------------------------
Publish-Project -Project 'TotalRecall\TotalRecall.csproj'         -OutSubdir 'TotalRecall'
Publish-Project -Project 'TotalRecall.Mcp\TotalRecall.Mcp.csproj' -OutSubdir 'TotalRecall.Mcp'

# --- 2. Drop README + LICENSE alongside the binaries --------------------------
Copy-Item -Path (Join-Path $root 'README.md') -Destination $stagingRoot
Copy-Item -Path (Join-Path $root 'LICENSE')   -Destination $stagingRoot

# --- 3. Write a tiny INSTALL.txt for non-technical users ---------------------
$installTxt = @"
TotalRecall $Version (win-x64, self-contained)

1. Extract this ZIP somewhere stable, e.g. C:\Tools\TotalRecall\
   (Do NOT run from a temp/download folder — the MCP path you wire into
   your AI agent must be permanent.)

2. Launch the app:
       TotalRecall\TotalRecall.exe

3. Configure encryption + capture interval on the Settings tab, click Save,
   then click Start on the Capture tab.

4. (Optional) Register the MCP server with your AI agent — see README.md
   section 'MCP server setup' for Microsoft Scout, GitHub Copilot CLI,
   and Claude Desktop wiring.

       MCP server exe: TotalRecall.Mcp\TotalRecall.Mcp.exe

Need help? See README.md or the project page at
   https://github.com/ilyafainberg/TotalRecall
"@
$installTxt | Set-Content -Path (Join-Path $stagingRoot 'INSTALL.txt') -Encoding UTF8

# --- 4. Zip everything ---------------------------------------------------------
Write-Host "Compressing -> $zipPath" -ForegroundColor Yellow
Compress-Archive -Path "$stagingRoot\*" -DestinationPath $zipPath -CompressionLevel Optimal -Force

# --- 5. Report ---------------------------------------------------------------
$zipSize = (Get-Item $zipPath).Length
$mb      = [math]::Round($zipSize / 1MB, 1)
Write-Host ""
Write-Host "Done." -ForegroundColor Green
Write-Host "  ZIP : $zipPath ($mb MB)"
Write-Host "  Staging tree left at: $stagingRoot"
