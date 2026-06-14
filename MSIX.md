# MSIX Package — TotalRecall

This doc covers the **MSIX** package: how it's built, how to sideload it for
testing, and how to submit it to the Microsoft Store.

> MSIX is the most "clean" install path on Windows — per-user, no admin
> required, no SmartScreen warning **once** the package is signed by a
> Microsoft-trusted certificate (either via the Store, or a real EV/OV code
> signing cert). The pre-built MSIX in the GitHub release is self-signed for
> sideload testing only; a Store-signed build will come once the listing is
> approved.

---

## Contents of the package

```
TotalRecall-1.0.0.0.msix
├─ AppxManifest.xml
├─ Assets\                       ← tile/splash PNGs
│   ├─ Square44x44Logo.png
│   ├─ Square71x71Logo.png
│   ├─ Square150x150Logo.png
│   ├─ Square310x310Logo.png
│   ├─ Wide310x150Logo.png
│   ├─ StoreLogo.png
│   └─ SplashScreen.png
├─ TotalRecall\                  ← WinForms GUI (self-contained .NET 10)
│   ├─ TotalRecall.exe
│   └─ … (deps + tessdata)
└─ TotalRecall.Mcp\              ← MCP server (self-contained .NET 10)
    ├─ TotalRecall.Mcp.exe       ← also exposed on PATH as
    │                             totalrecall-mcp.exe (App Execution Alias)
    └─ … (deps + tessdata)
```

### App Execution Alias

The manifest registers `totalrecall-mcp.exe` as a global alias for the
bundled MCP server, so MCP hosts (Microsoft Scout, GitHub Copilot CLI,
Claude Desktop, …) can launch it without having to know the locked-down
`%PROGRAMFILES%\WindowsApps\…` install dir.

After install, `where totalrecall-mcp.exe` should resolve from any shell.

---

## Building locally

Requires the bundled SDK build tools at `tools\sdk-buildtools\bin\<sdk-ver>\x64\`
(MakeAppx + signtool). The build script pulls them via NuGet:

```powershell
# One-time: fetch the Microsoft.Windows.SDK.BuildTools NuGet package
Invoke-WebRequest `
  "https://www.nuget.org/api/v2/package/Microsoft.Windows.SDK.BuildTools/10.0.26100.1" `
  -OutFile tools\sdk-buildtools\buildtools.zip
Expand-Archive tools\sdk-buildtools\buildtools.zip -DestinationPath tools\sdk-buildtools -Force
```

Then build:

```powershell
# Unsigned
.\msix\Build-Msix.ps1 -Version 1.0.0.0

# Self-signed (generates Cert:\CurrentUser\My cert + exports .cer / .pfx)
.\msix\Build-Msix.ps1 -Version 1.0.0.0 -Sign

# With your own PFX
.\msix\Build-Msix.ps1 -Version 1.0.0.0 -Sign `
  -CertPfx C:\path\to\my.pfx -CertPassword '...'
```

Output: `artifacts\msix\TotalRecall-<version>.msix`.

---

## Sideloading the self-signed MSIX

The release ships an MSIX signed with `CN=Ilya Fainberg` (self-signed).
Windows will refuse to install it until you trust that cert:

```powershell
# 1. Download both files from the release:
#    - TotalRecall-1.0.0.0.msix
#    - TotalRecall-1.0.0.0.cer

# 2. (Admin) Trust the cert
Import-Certificate `
  -FilePath .\TotalRecall-1.0.0.0.cer `
  -CertStoreLocation Cert:\LocalMachine\TrustedPeople

# 3. Install the package (no admin needed)
Add-AppxPackage -Path .\TotalRecall-1.0.0.0.msix
```

To uninstall:

```powershell
Get-AppxPackage IlyaFainberg.TotalRecall | Remove-AppxPackage
```

> **Why the manual cert trust?** Self-signed certs aren't in any root store.
> A Store-published or EV-signed build won't need this step — Windows trusts
> the Microsoft Store / DigiCert / Sectigo / etc. chains by default.

---

## Submitting to the Microsoft Store

The Store path is the one that **fully** eliminates SmartScreen for end users.
Here's the flow:

### 1. Reserve the app name

1. Sign in to [Partner Center](https://partner.microsoft.com/dashboard).
2. **Apps and games → New product → MSIX or PWA app**.
3. Reserve the name **TotalRecall** (or fall back to e.g. *Ilya's TotalRecall*
   if taken — Partner Center will show availability).
4. Partner Center will assign you a **Publisher CN** that looks like
   `CN=<long-guid>` and a **Package Identity Name** like
   `<publisherId>.TotalRecall`. **Write both down.**

### 2. Rebuild the package with Store identity

Edit `msix\Package.appxmanifest`:

```xml
<Identity
  Name="<publisherId>.TotalRecall"   <!-- from Partner Center -->
  Publisher="CN=<long-guid>"          <!-- from Partner Center -->
  Version="1.0.0.0"
  ProcessorArchitecture="x64" />

<Properties>
  <PublisherDisplayName>Ilya Fainberg</PublisherDisplayName>
  …
</Properties>
```

Then rebuild **unsigned**:

```powershell
.\msix\Build-Msix.ps1 -Version 1.0.0.0
```

(Don't sign — Partner Center signs the upload itself with the Store cert.)

### 3. Package as `.msixupload`

For the Store you upload a `.msixupload` file, which is just a ZIP containing
one or more `.msix` / `.msixbundle` files:

```powershell
$msix = "artifacts\msix\TotalRecall-1.0.0.0.msix"
$upload = "artifacts\msix\TotalRecall-1.0.0.0.msixupload"
Compress-Archive -Path $msix -DestinationPath $upload -Force
Rename-Item $upload -NewName "TotalRecall-1.0.0.0.msixupload"
```

### 4. Listing content

You'll need:

| Field             | Source                                                  |
|-------------------|---------------------------------------------------------|
| Display name      | TotalRecall                                             |
| Short description | "Local-first Windows screen indexer with OCR + MCP."    |
| Full description  | Reuse README intro + features list (markdown OK).       |
| Tagline           | "Capture, OCR, search — everything that happens on your PC." |
| Category          | Productivity                                            |
| Screenshots (≥3)  | 1366×768 or 1920×1080 PNGs — reuse the workspace shots  |
| Store logo        | `msix\Assets\StoreLogo.png`                             |
| Tile assets       | Already in the package — Store reuses them              |
| Age rating        | IARC questionnaire → expect **3+**                      |
| Privacy policy    | A URL with a privacy policy (required for any data-collecting app) |

### 5. Submit

Partner Center → **Packages** → upload the `.msixupload`. Then **Pricing &
availability** → Free, all markets. **Submit to the Store**.

Expected review time: **24–72 hours** for a first submission.

### 6. After approval

The listing URL will look like `https://apps.microsoft.com/detail/<id>` and
also be deep-linkable via `ms-windows-store://pdp/?productid=<id>`. Update
this repo's README to add a fourth install row pointing at the Store, and
push a new release that swaps the self-signed MSIX for the Store-signed one
(downloadable as a "sideload" backup from the listing).
