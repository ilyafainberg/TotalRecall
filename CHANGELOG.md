# TotalRecall v1.0.0

First public release. A local, searchable, encrypted index of everything that happens on your computer — with an optional MCP server so AI agents can query it.

## Highlights

- **100% on-device, no cloud, no telemetry.** Capture, OCR, storage, and search all run locally. No AI is in the loop unless *you* register the optional MCP server.
- **Encrypted-at-rest** SQLite via SQLCipher (AES-256). Three key modes: none, DPAPI-protected (silent unlock under your Windows account), or passphrase (prompted at launch).
- **WinForms desktop app** with three tabs: Capture, Browse, Settings. Light/dark friendly, system-tray, start-with-Windows.
- **Full-text search** over years of activity via SQLite FTS5 with highlighted snippets.
- **MCP server included** (`TotalRecall.Mcp.exe`) — 6 tools for AI agents (Microsoft Scout, GitHub Copilot CLI, Claude Desktop, any MCP-aware client).

## What's in the box

| Feature | Notes |
|---|---|
| Per-window screen capture | `PrintWindow` with `PW_RENDERFULLCONTENT`; honours window filters; visual-hash skips unchanged windows |
| OCR | Tesseract 5 (local). Configurable language(s), e.g. `eng+fra`. Drop additional `.traineddata` into `tessdata\`. |
| Storage | SQLite + SQLCipher. WAL mode. JPEG previews stored as BLOBs at configurable quality. |
| Search | FTS5 with `snippet()` highlighting; filter by app, date range, free text |
| Encryption | None / Windows-account (DPAPI) / passphrase |
| Retention | Strip images older than X days; delete rows older than Y days; auto-VACUUM after retention |
| Background mode | System tray, **Start at sign-in**, auto-resume capture |
| Capture state | Remembered across restarts; CLI flags `--capture-on` / `--capture-off` override |
| Command-line flags | `--tray`, `--capture-on`, `--capture-off`, `--minimized`, `--help` |
| MCP server | 6 tools: `search_recall`, `get_window`, `list_snapshots`, `get_snapshot`, `list_apps`, `stats` |
| License | **GPLv3** — Copyright © 2026 Ilya Fainberg |

## Install

Two install options for the full desktop app, plus a third for just the MCP server:

- **🟢 Inno Setup installer** *(recommended)* — `TotalRecall-1.0.0-Setup.exe` (~80 MB). Single-file, per-user install, no admin/UAC.
- **Portable ZIP** — `TotalRecall-1.0.0-Portable-win-x64.zip` (~116 MB). Extract anywhere stable, run `TotalRecall\TotalRecall.exe`. Self-contained, no install, no Add/Remove Programs entry.
- **MCP server only** — `TotalRecall-Mcp-1.0.0-win-x64.zip` (~59 MB). Standalone MCP server bundle for users who already have a TotalRecall DB and only need the AI-agent integration. Ships its own README + a paste-into-an-agent one-shot install prompt.

All packages are **self-contained** (no .NET runtime needed). Both the Inno Setup installer and the Portable ZIP trigger SmartScreen on first launch until the binaries are Authenticode-signed — *More info → Run anyway*.

### First-run setup

1. On the **Settings** tab, pick an encryption mode and capture interval. Click **Save**.
2. On the **Capture** tab, click **Start**.
3. (Optional) Wire the MCP server into your AI agent — see [README.md](README.md#mcp-server-setup) for Scout, GitHub Copilot CLI, and Claude Desktop configs.

## Requirements

- Windows 10 or 11, x64
- ~120 MB free disk space for the install, plus database growth (~1–5 MB / hour of active capture at default settings)
- **No .NET runtime required** — the ZIP is self-contained

## Known caveats

- DRM-protected video surfaces and some hardware-accelerated games come back as black frames from `PrintWindow`. Metadata is still recorded.
- The self-contained ZIP includes the .NET 10 runtime — nothing else to install. Building from source requires the .NET 10 SDK.
- App allow-list / block-list isn't implemented yet — if you don't want a window captured, minimize it or pause capture.

## Thanks

Tesseract, SQLite + SQLCipher, ModelContextProtocol C# SDK, and the .NET team for WinForms-on-.NET-10 actually being pleasant.
