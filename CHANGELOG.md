# TotalRecall v1.2.0

A big browse-experience + performance release. The Browse pane is faster and simpler on large databases, screenshots load lazily, and there's an in-app updater.

## What's new

### Search & browse
- **Simpler, faster search.** The fiddly start-date + count + Day/Week/Month controls are gone. There's now a single **Time range** dropdown — *Any time · Today · Last 24 hours · Last 7 days · Last 30 days · This year · Custom…* — that defaults to **Last 7 days** so opening the app never scans your whole database. Picking a range searches immediately (no separate Filter button). **Custom…** reveals two date pickers and tolerates an inverted from/to range.
- **Sortable columns.** Click the **When / Title / App** headers to sort, with a ▲/▼ indicator.
- **Clear loading state.** While a query runs, the panels clear and show a plain **"Loading…"** label instead of a spinning wait-cursor, and overlapping searches (e.g. rapid range changes) no longer get stuck in a loading loop.
- **Single Start / Stop button.** The two header buttons are now one toggle, and **Quit** moved into the **☰** menu.

### Performance
- **Screenshots split into their own table.** JPEG blobs moved out of the main `windows` table into a 1:1 `window_images` side table. The `windows` b-tree stays small, so metadata/search scans are dramatically faster on multi-GB databases. Existing databases keep working — old inline images still render via a fallback.
- **Lazy image loading.** The results list and search never carry image bytes; a screenshot is fetched only when you actually select a row.
- **Query tuning.** Newest-first ordering now walks the primary key instead of sorting a joined column, plus `temp_store=MEMORY` and a larger page cache — the difference between an instant Browse load and a multi-second hang on a large DB.

### Updates & polish
- **In-app updater.** **☰ → Check for updates…** queries the GitHub Releases API, and downloads the new portable build with a live progress bar.
- **Clickable About** with the project link and author credit.
- **Result limit moved to Settings** (out of the search bar).
- **JPEG-quality slider ignores the mouse wheel** so scrolling Settings can't nudge it; the **Store JPEGs** checkbox is no longer clipped.
- **Cleaner splitter** — the chunky Win32 "barbell" cursor is replaced by the standard resize cursor, shown only over the divider.
- **Activity log** now trims to the last 4,000 lines (matching its ring buffer).
- **Corrected encryption note** — changing the encryption mode/passphrase re-encrypts the existing database in place.

## Artifacts
- **🟢 Inno Setup installer** *(recommended)* — `TotalRecall-1.2.0-Setup.zip`. Unzip, then run the installer inside. Single-file, per-user install, no admin/UAC. *(Shipped inside a ZIP so SmartScreen / Edge / Chrome don't flag the raw `.exe` as "not commonly downloaded".)*
- **Portable ZIP** — `TotalRecall-1.2.0-Portable-win-x64.zip`. Extract anywhere stable, run `TotalRecall\TotalRecall.exe`. Self-contained, no install, no Add/Remove Programs entry.
- **MCP server only** — `TotalRecall-Mcp-1.2.0-win-x64.zip`. Standalone MCP server bundle for users who already have a TotalRecall DB and only need the AI-agent integration.

All artifacts are self-contained — no .NET runtime required.

## Upgrading from 1.1.x
`settings.json` and the MCP tool surface are unchanged. The database schema gains the `window_images` table automatically on first open; your existing captures (including old screenshots) keep working. New captures store images in the side table — the existing file only shrinks as retention runs.

---

# TotalRecall v1.1.1

UX polish + stability fixes that surfaced after v1.1.0 went live.

## What's new

- **Smooth tray transitions.** Hiding the window to the system tray no longer flickers (window briefly reappearing before vanishing) and restoring from the tray no longer suffers a slow, staggered repaint. The previous code toggled `Form.ShowInTaskbar`, which forces WinForms to destroy and recreate the HWND of every control in the window — that's gone now, replaced by a plain `Hide()` / `Show()` pair.
- **No more Save & Close stutter / crash.** Saving Settings without changing the encryption mode used to trigger a full capture-stop, services teardown, DB reopen, and capture restart — freezing the UI for up to 10 seconds and occasionally crashing on a second Save. The Settings handler now takes a fast path when nothing encryption-related changed: it just repaints the header summary and refreshes the Browse panel. The full teardown/rekey/restart cycle only fires when the encryption mode or passphrase actually changes.
- **Window title is just "TotalRecall".** The encryption-state suffix in the title bar (`TotalRecall (User Account Encrypted)`) was only refreshed at startup, so it would lie after any encryption change. Removed entirely — current encryption state lives in the header summary, which is always up to date.

## Artifacts

- **🟢 Inno Setup installer** *(recommended)* — `TotalRecall-1.1.1-Setup.zip`. Unzip, then run the installer inside. Single-file, per-user install, no admin/UAC. *(Shipped inside a ZIP so SmartScreen / Edge / Chrome don't flag the raw `.exe` as "not commonly downloaded".)*
- **Portable ZIP** — `TotalRecall-1.1.1-Portable-win-x64.zip`. Extract anywhere stable, run `TotalRecall\TotalRecall.exe`. Self-contained, no install, no Add/Remove Programs entry.
- **MCP server only** — `TotalRecall-Mcp-1.1.1-win-x64.zip`. Standalone MCP server bundle for users who already have a TotalRecall DB and only need the AI-agent integration.

All artifacts are self-contained — no .NET runtime required.

## Upgrading from 1.1.0

Database schema, settings.json, and the MCP tool surface are unchanged. Drop the new build over the old one and your existing DB will open as-is.

---

# TotalRecall v1.1.0

Stability release focused on encryption changes, shutdown safety, and smoother window dragging. If you ever changed the encryption mode in v1.0.x and ended up staring at a hung window or an "unable to open database" error, this is the fix.

## What's new

- **Modal re-encryption with progress.** Switching the encryption mode (None / User Account / Password) on a 100+ MB database used to freeze the UI for tens of seconds with no feedback. Now you get a proper modal dialog with a real progress bar driven by the rekey's temp-file growth, plus a Quit button that lets you walk away — the rekey keeps running and the app exits cleanly when it's done.
- **Encryption-change bugfix.** Saving Settings with a new encryption mode could leave the database in a state that wouldn't open on the next launch (`SQLite Error 14: unable to open database`). The rekey path now pre-seeds the destination file with the correct cipher state so SQLCipher's `ATTACH DATABASE` accepts it in both directions (encrypted ↔ plaintext).
- **Auto-refresh restored after rekey.** After a successful re-encryption, the results list now repopulates immediately and capture resumes if it was running — no more empty Browse pane until the next manual F5.
- **Settings: Save & Close + Cancel.** Settings now has explicit `Save & Close` and `Cancel` buttons. Esc cancels. No more "did anything happen" after clicking save.
- **Smoother window dragging.** Reduced UI work during the WM_NCLBUTTONDOWN / move loop so dragging the window no longer stutters when capture is running.
- **Graceful shutdown.** Closing the app while a capture tick or retention sweep is in flight now waits up to 5 seconds for those tasks to drain before disposing the database. Fixes the occasional "background tasks still running" crash on exit.
- **Persistent app log.** Every activity-log line is also mirrored to `%LOCALAPPDATA%\TotalRecall\app.log` (auto-rolled at 1 MB) and unhandled exceptions on every thread are now logged. Makes post-mortem debugging actually possible.

## Artifacts

- **🟢 Inno Setup installer** *(recommended)* — `TotalRecall-1.1.0-Setup.zip`. Unzip, then run the installer inside. Single-file, per-user install, no admin/UAC. *(Shipped inside a ZIP so SmartScreen / Edge / Chrome don't flag the raw `.exe` as "not commonly downloaded".)*
- **Portable ZIP** — `TotalRecall-1.1.0-Portable-win-x64.zip`. Extract anywhere stable, run `TotalRecall\TotalRecall.exe`. Self-contained, no install, no Add/Remove Programs entry.
- **MCP server only** — `TotalRecall-Mcp-1.1.0-win-x64.zip`. Standalone MCP server bundle for users who already have a TotalRecall DB and only need the AI-agent integration.

All artifacts are self-contained — no .NET runtime required.

## Upgrading from 1.0.x

Database schema, settings.json, and the MCP tool surface are unchanged. Drop the new build over the old one and your existing DB will open as-is.

---

# TotalRecall v1.0.1

Quality-of-life release. Mostly UX polish plus an OSS-readiness comment sweep across the codebase so external contributors can find their footing.

## What's new

- **Single-window UI overhaul.** The old "Capture / Browse / Settings" tabbed layout is gone. The window is now Browse-first, with:
  - A compact capture status bar inlined directly into the application header (status pill, interval / JPEG / encryption summary, last-snapshot stamp, Start/Stop).
  - A three-pane Browse workspace: results · zoomable preview · collapsible OCR text.
  - Activity Log, Settings, and Open DB Folder moved behind a hamburger menu in the header.
  - "Capture Now" removed (it was a debug button that confused users).
- **Zoomable preview with pan.** The preview pane now supports Fit / 25 / 50 / 75 / 100 / 150 / 200% zoom via a slider, `Ctrl + +` / `Ctrl + -` / `Ctrl + 0`, and `Ctrl + mouse wheel`. When zoomed past the viewport, the hand cursor lets you click-and-drag to pan the image.
- **Keyboard-shortcut display fix.** Hamburger menu items now use `ShortcutKeyDisplayString` (the correct WinForms property) so the keybindings actually render on the right of each item.
- **OSS contributor pass.** Every non-Designer source file received a class-level XML summary explaining its role, rationale, and extension points. `AppSettings` now has a "how to add a setting" checklist; `Database` has a schema overview; `SnapshotService` has an algorithm narrative explaining why change-detection runs before OCR.

## Artifacts

- **🟢 Inno Setup installer** *(recommended)* — `TotalRecall-1.0.1-Setup.exe`. Single-file, per-user install, no admin/UAC.
- **Portable ZIP** — `TotalRecall-1.0.1-Portable-win-x64.zip`. Extract anywhere stable, run `TotalRecall\TotalRecall.exe`. Self-contained, no install, no Add/Remove Programs entry.
- **MCP server only** — `TotalRecall-Mcp-1.0.1-win-x64.zip`. Standalone MCP server bundle for users who already have a TotalRecall DB and only need the AI-agent integration.

All artifacts are self-contained — no .NET runtime required.

## No breaking changes

Database schema, settings.json, and the MCP tool surface are unchanged from v1.0.0.

---

# TotalRecall v1.0.0

First public release. A local, searchable, encrypted index of everything that happens on your computer — with an optional MCP server so AI agents can query it.

## Highlights

- **100% on-device, no cloud, no telemetry.** Capture, OCR, storage, and search all run locally. No AI is in the loop unless *you* register the optional MCP server.
- **Encrypted-at-rest** SQLite via SQLCipher (AES-256). Three key modes: none, DPAPI-protected (silent unlock under your Windows account), or passphrase (prompted at launch).
- **WinForms desktop app** — single-window layout with a compact capture bar in the header, a 3-pane Browse workspace (results · zoomable preview · collapsible text), and Activity Log / Settings as separate windows behind a hamburger menu. Light theme, system-tray, start-with-Windows.
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

