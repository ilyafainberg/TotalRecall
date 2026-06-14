#requires -Version 5.1
<#
.SYNOPSIS
    Builds a TotalRecall .msix package from the portable staging tree.

.DESCRIPTION
    Assumes build-release.ps1 has already produced:
        artifacts\staging\TotalRecall-1.0.0-win-x64\
            TotalRecall\        (self-contained WinForms app)
            TotalRecall.Mcp\    (self-contained MCP server)
            README.md / LICENSE / INSTALL.txt

    Stages a MSIX layout, runs MakeAppx, and (optionally) signs with a
    self-signed cert for local sideload smoke-testing.

.PARAMETER Version
    4-part version baked into the manifest. Default 1.0.0.0.

.PARAMETER Sign
    If specified, generates (or reuses) a self-signed cert in
    Cert:\CurrentUser\My with Subject "CN=Ilya Fainberg" and signs
    the resulting .msix with signtool.

.PARAMETER CertPfx / CertPassword
    Use an existing PFX instead of generating a self-signed cert.
#>
[CmdletBinding()]
param(
    [string]$Version  = "1.0.0.0",
    [switch]$Sign,
    [string]$CertPfx  = "",
    [string]$CertPassword = ""
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

# ---------------------------------------------------------------------------
# 0. Resolve SDK tools
# ---------------------------------------------------------------------------
$SdkRoot = Join-Path $Root "tools\sdk-buildtools\bin"
$SdkVer  = (Get-ChildItem $SdkRoot -Directory | Sort-Object Name -Descending | Select-Object -First 1).Name
$MakeAppx = Join-Path $SdkRoot "$SdkVer\x64\makeappx.exe"
$SignTool = Join-Path $SdkRoot "$SdkVer\x64\signtool.exe"

if (-not (Test-Path $MakeAppx)) { throw "MakeAppx.exe not found under $SdkRoot" }
if ($Sign -and -not (Test-Path $SignTool)) { throw "signtool.exe not found under $SdkRoot" }

Write-Host "→ Using MakeAppx: $MakeAppx"
Write-Host "→ Using SignTool: $SignTool"

# ---------------------------------------------------------------------------
# 1. Stage MSIX layout
# ---------------------------------------------------------------------------
$Portable = Join-Path $Root "artifacts\staging\TotalRecall-1.0.0-win-x64"
if (-not (Test-Path $Portable)) {
    throw "Portable staging tree missing at $Portable. Run build-release.ps1 first."
}

$Staging = Join-Path $Root "artifacts\staging\TotalRecall-$Version-MSIX"
if (Test-Path $Staging) { Remove-Item $Staging -Recurse -Force }
New-Item -ItemType Directory -Path $Staging -Force | Out-Null

Write-Host "`n→ Staging MSIX layout at $Staging"
Copy-Item (Join-Path $Portable "TotalRecall")     (Join-Path $Staging "TotalRecall")     -Recurse
Copy-Item (Join-Path $Portable "TotalRecall.Mcp") (Join-Path $Staging "TotalRecall.Mcp") -Recurse

# Assets + manifest at the root
Copy-Item (Join-Path $PSScriptRoot "Assets") (Join-Path $Staging "Assets") -Recurse

# Patch the manifest's version on the way in so the file on disk stays generic.
$ManifestSrc = Join-Path $PSScriptRoot "Package.appxmanifest"
# MakeAppx requires the file inside the staging dir to be named AppxManifest.xml
$ManifestDst = Join-Path $Staging "AppxManifest.xml"
$xml = Get-Content $ManifestSrc -Raw
$xml = $xml -replace 'Version="1\.0\.0\.0"', "Version=`"$Version`""
Set-Content -Path $ManifestDst -Value $xml -Encoding UTF8

Write-Host "  ✓ TotalRecall\          ($((Get-ChildItem (Join-Path $Staging 'TotalRecall') -Recurse -File).Count) files)"
Write-Host "  ✓ TotalRecall.Mcp\      ($((Get-ChildItem (Join-Path $Staging 'TotalRecall.Mcp') -Recurse -File).Count) files)"
Write-Host "  ✓ Assets\               ($((Get-ChildItem (Join-Path $Staging 'Assets') -File).Count) tiles)"
Write-Host "  ✓ AppxManifest.xml      (Version=$Version)"

# ---------------------------------------------------------------------------
# 2. Build the .msix
# ---------------------------------------------------------------------------
$OutDir = Join-Path $Root "artifacts\msix"
New-Item -ItemType Directory -Path $OutDir -Force | Out-Null
$MsixPath = Join-Path $OutDir "TotalRecall-$Version.msix"
if (Test-Path $MsixPath) { Remove-Item $MsixPath -Force }

Write-Host "`n→ Building MSIX → $MsixPath"
& $MakeAppx pack /d $Staging /p $MsixPath /o
if ($LASTEXITCODE -ne 0) { throw "MakeAppx pack failed with exit code $LASTEXITCODE" }

$msixSize = "{0:N1} MB" -f ((Get-Item $MsixPath).Length / 1MB)
Write-Host "  ✓ $MsixPath ($msixSize)"

# ---------------------------------------------------------------------------
# 3. Sign (optional)
# ---------------------------------------------------------------------------
if ($Sign) {
    Write-Host "`n→ Signing MSIX"

    if ($CertPfx -ne "") {
        if (-not (Test-Path $CertPfx)) { throw "Cert PFX not found at $CertPfx" }
        & $SignTool sign /fd SHA256 /f $CertPfx /p $CertPassword $MsixPath
    }
    else {
        # Generate self-signed cert in CurrentUser\My (one-shot, idempotent)
        $cert = Get-ChildItem Cert:\CurrentUser\My |
                Where-Object { $_.Subject -eq "CN=Ilya Fainberg" } |
                Select-Object -First 1
        if (-not $cert) {
            Write-Host "  · Generating self-signed cert CN=Ilya Fainberg"
            $cert = New-SelfSignedCertificate `
                -Type CodeSigningCert `
                -Subject "CN=Ilya Fainberg" `
                -KeyUsage DigitalSignature `
                -FriendlyName "TotalRecall Sideload Cert" `
                -CertStoreLocation "Cert:\CurrentUser\My" `
                -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
        }
        Write-Host "  · Using cert thumbprint $($cert.Thumbprint)"

        # Export to PFX
        $pfxPath = Join-Path $OutDir "TotalRecall-SelfSigned.pfx"
        $pwd     = ConvertTo-SecureString "totalrecall" -AsPlainText -Force
        $null    = Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $pwd -Force

        # Export public CER for the user to trust (LocalMachine\TrustedPeople)
        $cerPath = Join-Path $OutDir "TotalRecall-SelfSigned.cer"
        $null    = Export-Certificate -Cert $cert -FilePath $cerPath -Force

        & $SignTool sign /fd SHA256 /f $pfxPath /p "totalrecall" $MsixPath
    }

    if ($LASTEXITCODE -ne 0) { throw "signtool failed with exit code $LASTEXITCODE" }
    Write-Host "  ✓ Signed."

    Write-Host "`nTo sideload:"
    Write-Host "  1. (Admin) Import-Certificate -FilePath `"$OutDir\TotalRecall-SelfSigned.cer`" -CertStoreLocation Cert:\LocalMachine\TrustedPeople"
    Write-Host "  2. Add-AppxPackage -Path `"$MsixPath`""
}
else {
    Write-Host "`n(skipped signing — use -Sign to self-sign for local install)"
}

Write-Host "`nMSIX ready: $MsixPath"
