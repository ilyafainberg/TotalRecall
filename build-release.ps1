# ============================================================================
#  build-release.ps1 - produce the TotalRecall release artifacts locally.
#
#  Reproduces the exact three assets that ship on each GitHub Release:
#    1. TotalRecall-<ver>-Portable-win-x64.zip   (unzip-and-run UI + MCP + docs)
#    2. TotalRecall-<ver>-Setup.zip              (Inno Setup installer, zipped)
#    3. TotalRecall-Mcp-<ver>-win-x64.zip        (standalone MCP server)
#
#  All builds are self-contained win-x64 (no .NET runtime required on the
#  target machine). The installer is built with Inno Setup 6 (ISCC.exe), which
#  must be installed; the script auto-discovers it in the usual locations.
#
#  Usage:
#    .\build-release.ps1 -Version 1.2.0
#    .\build-release.ps1 -Version 1.2.0 -SkipInstaller   # portable + MCP only
#
#  Output lands in .\artifacts\ (staging/ + portable/) and the three *.zip
#  files are copied to the repo root next to this script.
# ============================================================================
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Version,

    # Skip the Inno Setup step (e.g. on a machine without ISCC installed).
    [switch] $SkipInstaller
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
Set-Location $root

$uiProj  = Join-Path $root 'TotalRecall\TotalRecall.csproj'
$mcpProj = Join-Path $root 'TotalRecall.Mcp\TotalRecall.Mcp.csproj'

$artifacts = Join-Path $root 'artifacts'
$stageRoot = Join-Path $artifacts "staging\TotalRecall-$Version-win-x64"
$stageUi   = Join-Path $stageRoot 'TotalRecall'
$stageMcp  = Join-Path $stageRoot 'TotalRecall.Mcp'
$portDir   = Join-Path $artifacts "portable\TotalRecall-$Version-Portable-win-x64"

function Write-Step([string] $msg) { Write-Host "`n=== $msg ===" -ForegroundColor Cyan }

# --- 0. Clean -----------------------------------------------------------------
Write-Step "Cleaning previous staging for $Version"
foreach ($p in @($stageRoot, $portDir)) {
    if (Test-Path $p) { Remove-Item -Recurse -Force $p }
}
New-Item -ItemType Directory -Force -Path $stageRoot, $portDir | Out-Null

# The UI .csproj bundles the MCP publish into a McpServer\ subfolder IF the MCP
# project's *default* publish folder exists. We keep the MCP separate in the
# release layout, so publish the UI FIRST and make sure that default folder is
# absent so it never gets embedded.
$mcpDefaultPublish = Join-Path $root 'TotalRecall.Mcp\bin\Release\net10.0-windows\win-x64\publish'
if (Test-Path $mcpDefaultPublish) { Remove-Item -Recurse -Force $mcpDefaultPublish }

# --- 1. Publish the WinForms UI (self-contained) ------------------------------
Write-Step "Publishing UI -> $stageUi"
dotnet publish $uiProj -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=false -o $stageUi
if ($LASTEXITCODE -ne 0) { throw "UI publish failed." }

# --- 2. Publish the MCP server (self-contained) -------------------------------
Write-Step "Publishing MCP server -> $stageMcp"
dotnet publish $mcpProj -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=false -o $stageMcp
if ($LASTEXITCODE -ne 0) { throw "MCP publish failed." }

# --- 3. Assemble the portable folder ------------------------------------------
Write-Step "Assembling portable folder"
Copy-Item $stageUi  (Join-Path $portDir 'TotalRecall')     -Recurse -Force
Copy-Item $stageMcp (Join-Path $portDir 'TotalRecall.Mcp') -Recurse -Force
Copy-Item (Join-Path $root 'LICENSE')   $portDir -Force
Copy-Item (Join-Path $root 'README.md') $portDir -Force

@"
TotalRecall $Version - Portable
============================

To run:
  1. Extract the entire folder anywhere stable (e.g. C:\Tools\TotalRecall).
  2. Double-click TotalRecall\TotalRecall.exe.

To register the MCP server with an MCP-aware client (e.g. Claude Desktop, Copilot CLI):
  Point the client at TotalRecall.Mcp\TotalRecall.Mcp.exe.

No installer. No Add/Remove Programs entry. Delete the folder to uninstall.
"@ | Set-Content (Join-Path $portDir 'INSTALL.txt') -Encoding UTF8

# --- 4. Zip the portable + MCP-only artifacts ---------------------------------
Write-Step "Zipping portable + MCP artifacts"
$portableZip = Join-Path $root "TotalRecall-$Version-Portable-win-x64.zip"
$mcpZip      = Join-Path $root "TotalRecall-Mcp-$Version-win-x64.zip"
if (Test-Path $portableZip) { Remove-Item -Force $portableZip }
if (Test-Path $mcpZip)      { Remove-Item -Force $mcpZip }

Compress-Archive -Path (Join-Path $portDir '*') -DestinationPath $portableZip -CompressionLevel Optimal
Compress-Archive -Path (Join-Path $stageMcp '*') -DestinationPath $mcpZip -CompressionLevel Optimal

# --- 5. Build the Inno Setup installer (and zip it) ---------------------------
if (-not $SkipInstaller) {
    Write-Step "Building Inno Setup installer"
    $iscc = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1

    if (-not $iscc) {
        throw "Inno Setup (ISCC.exe) not found. Install it, or pass -SkipInstaller."
    }

    & $iscc /Qp (Join-Path $root 'installer\TotalRecall.iss') "/DMyAppVersion=$Version"
    if ($LASTEXITCODE -ne 0) { throw "Inno Setup compile failed." }

    $setupExe = Join-Path $root "installer\Output\TotalRecall-$Version-Setup.exe"
    if (-not (Test-Path $setupExe)) { throw "Expected installer not found: $setupExe" }

    $setupZip = Join-Path $root "TotalRecall-$Version-Setup.zip"
    if (Test-Path $setupZip) { Remove-Item -Force $setupZip }
    Compress-Archive -Path $setupExe -DestinationPath $setupZip -CompressionLevel Optimal
}

# --- 6. Summary ---------------------------------------------------------------
Write-Step "Done - release artifacts"
Get-ChildItem $root -Filter "TotalRecall*$Version*.zip" |
    Select-Object Name, @{n = 'SizeMB'; e = { [math]::Round($_.Length / 1MB, 1) } } |
    Format-Table -AutoSize
