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
$clickOnceZip = Join-Path $root "TotalRecall-$Version-ClickOnce.zip"

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
Write-Host "Done (portable ZIP)." -ForegroundColor Green
Write-Host "  ZIP : $zipPath ($mb MB)"
Write-Host "  Staging tree left at: $stagingRoot"

# --- 6. ClickOnce installer (single setup.exe + manifest) --------------------
Write-Host ""
Write-Host "Building ClickOnce installer..." -ForegroundColor Cyan
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) {
    Write-Host "  vswhere.exe not found — skipping ClickOnce build (install Visual Studio to enable)." -ForegroundColor Yellow
    return
}
$msbuild = & $vswhere -latest -prerelease -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
if (-not $msbuild) {
    Write-Host "  MSBuild.exe not found via vswhere — skipping ClickOnce build." -ForegroundColor Yellow
    return
}
Write-Host "  Using MSBuild: $msbuild"

# Pre-publish the MCP server into the location the WinForms csproj globs for.
$mcpPublishOut = Join-Path $root "TotalRecall.Mcp\bin\$Configuration\net10.0-windows\$rid\publish"
if (Test-Path $mcpPublishOut) { Remove-Item $mcpPublishOut -Recurse -Force }
& dotnet publish (Join-Path $root 'TotalRecall.Mcp\TotalRecall.Mcp.csproj') `
    -c $Configuration -r $rid --self-contained true `
    -p:PublishSingleFile=false -p:DebugType=none -p:DebugSymbols=false `
    -o $mcpPublishOut --nologo | Out-Null
if ($LASTEXITCODE -ne 0) { throw "MCP pre-publish failed" }

# Clean previous ClickOnce output
$coOutDir = Join-Path $root 'TotalRecall\bin\publish'
if (Test-Path $coOutDir) { Remove-Item $coOutDir -Recurse -Force }

# Run ClickOnce publish
$msbuildArgs = @(
    (Join-Path $root 'TotalRecall\TotalRecall.csproj'),
    '/t:Publish',
    "/p:Configuration=$Configuration",
    '/p:Platform=x64',
    '/p:PublishProfile=ClickOnceProfile',
    "/p:ApplicationVersion=$Version.0",
    '/v:minimal',
    '/nologo'
)
& $msbuild @msbuildArgs
if ($LASTEXITCODE -ne 0) { throw "ClickOnce publish failed (exit $LASTEXITCODE)" }

if (-not (Test-Path $coOutDir)) {
    Write-Host "  ClickOnce output not found at $coOutDir" -ForegroundColor Yellow
    return
}

# Zip the entire ClickOnce publish folder (setup.exe + .application + Application Files\ + publish.html)
if (Test-Path $clickOnceZip) { Remove-Item $clickOnceZip -Force }
Write-Host "Compressing ClickOnce -> $clickOnceZip" -ForegroundColor Yellow
Compress-Archive -Path "$coOutDir\*" -DestinationPath $clickOnceZip -CompressionLevel Optimal -Force

$coMb = [math]::Round((Get-Item $clickOnceZip).Length / 1MB, 1)
Write-Host ""
Write-Host "Done (ClickOnce)." -ForegroundColor Green
Write-Host "  ZIP    : $clickOnceZip ($coMb MB)"
Write-Host "  setup  : $coOutDir\setup.exe"
Write-Host "  manifest: $coOutDir\TotalRecall.application"
