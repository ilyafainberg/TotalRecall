# TotalRecall

> A local, searchable, encrypted index of **everything that happens on your computer** — built on .NET 10, fully open source, **100% on-device**, with an optional MCP server so AI agents can query it.

---

## What it does

Every N seconds (default: 10s), TotalRecall takes a screenshot of visible windows on your screen, skips windows that have not visually changed, runs OCR over changed images to extract the text, and stores the result in a local SQLite database:

- Window title and app name
- Process name + executable path
- Window bounds and whether it was in the foreground
- The full OCR text of what was visible
- The window screenshot itself, stored as a JPEG blob

You can then:

- **Browse** every snapshot in a dark-themed WinForms UI with a JPEG preview pane and full text view.
- **Search** across years of activity in milliseconds using SQLite **FTS5** full-text search (with highlighted snippets).
- **Filter** by app, date range, or both.
- Optionally expose the database to AI agents over the **Model Context Protocol** so an assistant can answer questions like *"what was that invoice number I was looking at last Tuesday?"*

The vision is a personal, private "computer memory" — Microsoft Recall, but open source and entirely yours.

---

## Install

There are two ways to install TotalRecall — pick whichever you prefer:

### Option 1 — ClickOnce installer (recommended)

The OneClick installer is the easiest path. No admin rights, no UAC prompt, automatic Start Menu shortcut, per-user install.

1. Download **`TotalRecall-1.0.0-ClickOnce.zip`** from <https://github.com/ilyafainberg/TotalRecall/releases>.
2. Extract the ZIP somewhere temporary (e.g. Downloads is fine — only `setup.exe` matters after install).
3. Double-click **`setup.exe`**.
4. Windows SmartScreen will say *"Windows protected your PC"* because this build isn't Authenticode-signed yet — click **More info → Run anyway**.
5. The app installs into `%LOCALAPPDATA%\Apps\2.0\…\` and a Start Menu shortcut is created. Launch *TotalRecall* from the Start Menu.

The MCP server (`TotalRecall.Mcp.exe`) is bundled inside the install. To find its current absolute path (it's hash-based and changes between updates), launch TotalRecall and check the activity log on the Capture tab — the path is printed at startup.

### Option 2 — Portable ZIP

If you prefer to control where the app lives (e.g. on a USB stick or under `C:\Tools\`), grab the portable ZIP — no install, no Start Menu entry.

1. Download **`TotalRecall-1.0.0-win-x64.zip`** from the same release page.
2. Extract somewhere **stable** (e.g. `C:\Tools\TotalRecall\`). Don't run from Downloads — the MCP path you wire into your AI agent must be permanent.
3. Run `TotalRecall\TotalRecall.exe`.

The release ZIP includes **both** the desktop app and the MCP server side by side:

```
TotalRecall-1.0.0-win-x64\
├─ TotalRecall\
│  └─ TotalRecall.exe          ← the desktop app
├─ TotalRecall.Mcp\
│  └─ TotalRecall.Mcp.exe      ← the MCP server (see "MCP server setup" below)
├─ README.md
├─ INSTALL.txt
└─ LICENSE
```

### First-run setup (either option)

1. On the **Settings** tab, pick an encryption mode (default is **Windows account** — DPAPI-encrypted, silent unlock) and a capture interval (default 10 s). Click **Save**.
2. On the **Capture** tab, click **Start**.
3. (Optional) Wire `TotalRecall.Mcp.exe` into your AI agent — see [MCP server setup](#mcp-server-setup) for Microsoft Scout, GitHub Copilot CLI, and Claude Desktop wiring.

Both packages are **self-contained** — no separate .NET runtime installation required.

### Command-line flags

| Flag | Effect |
|------|--------|
| `--tray` | Launch hidden in the system tray (use with **Settings → Minimize to tray** enabled). |
| `--capture-on` | Force capture to start regardless of last remembered state. |
| `--capture-off` | Force capture to stay stopped regardless of last remembered state. |
| `--minimized` / `-m` | Start minimized (taskbar, or tray if tray mode is on). |
| `--help` / `-h` / `/?` | Print usage and exit. |

If neither `--capture-on` nor `--capture-off` is passed, TotalRecall restores whatever capture state it had when it was last closed.

---

## On-device, open source, no AI in the loop

This is a deliberate design choice. The capture pipeline does **not** call any cloud service. Specifically:

- **Screenshots** are captured via Win32 `PrintWindow` — pixels never leave your machine.
- **OCR** runs locally with **Tesseract 5** (Apache 2.0). No vision API, no cloud OCR, no telemetry.
- **Storage** is a local SQLite file encrypted at rest with **SQLCipher** (BSD/MIT).
- **Search** is local SQLite FTS5.

No AI model, LLM, or cloud service is involved in capture, OCR, storage, or search.

The **only** time AI becomes involved is when *you* choose to register the optional MCP server with your AI assistant (Claude Desktop, GitHub Copilot CLI, Microsoft Scout, etc.). In that case the agent calls the MCP server's tools, which return rows from your local database. The database still never leaves the machine — only the specific query results you asked the agent to fetch are sent to the model you've configured.

If you never enable the MCP server, no AI ever sees a byte of your data.

### Licensing

**TotalRecall is licensed under the GNU General Public License v3.0** (or any later version, at your option) — see the [LICENSE](LICENSE) file for the full text. Copyright © 2026 Ilya Fainberg. This is **strong copyleft**: you can use, study, share, and modify the code freely, but any work you distribute that incorporates TotalRecall must also be released under GPLv3.

Dependencies and their licenses:

| Dependency | License |
|---|---|
| .NET 10 / WinForms | MIT |
| Tesseract (.NET wrapper + native Tesseract / Leptonica) | Apache 2.0 |
| Microsoft.Data.Sqlite.Core | MIT |
| SQLitePCLRaw.bundle_e_sqlcipher (SQLCipher) | BSD-3 (wrapper) + zlib/MIT (SQLCipher) |
| ModelContextProtocol (C# SDK) | MIT |

All of the above are compatible with GPLv3 distribution.

---

## How it works

### Solution layout

```
TotalRecall.sln
├── TotalRecall.Core/   class library — settings, key vault, DB schema, OCR,
│                       window enumeration, screenshot, snapshot service
├── TotalRecall/        WinForms UI (3 tabs, dark theme)
└── TotalRecall.Mcp/    stdio MCP server (6 tools)
```

### Capture pipeline (per tick)

1. **Enumerate windows** — `EnumWindows` + filters: skip invisible, minimized, DWM-cloaked (hidden UWP shells), tool windows, and anything < 32×32 px or without a title. Process metadata is cached across ticks.
2. **Capture pixels** — `PrintWindow(hWnd, hdc, PW_RENDERFULLCONTENT)` against a `Bitmap`. Works with modern apps using DirectComposition; a few hardware-accelerated surfaces (some games, DRM-protected video) may come back black — metadata is still recorded.
3. **Detect change** — A lightweight sampled visual hash skips unchanged windows before OCR, JPEG encoding, DB inserts, and FTS indexing.
4. **Identify the app** — Resolve PID → `Process` → `FileVersionInfo.FileDescription` for a friendly app name; fall back to `ProcessName`.
5. **OCR** — A single shared `TesseractEngine` (configurable language) runs over a grayscale, capped-size bitmap. Serialized through a lock to keep the native library happy.
6. **Encode JPEG** — Quality is configurable (30–95). The original lossless bitmap is used for OCR, then the JPEG is created purely for storage and preview.
7. **Insert row** — One `snapshots` row + N `windows` rows in a single SQLite transaction. FTS5 triggers keep the search index in sync.

### Database schema

```
snapshots(id, ts, ts_unix, user, machine, window_count, elapsed_ms)

windows(id, snapshot_id → snapshots.id,
        title, app_name, process_name, process_id, executable_path,
        is_foreground,
        bounds_x, bounds_y, bounds_w, bounds_h,
        text, ocr_error, ocr_duration_ms,
        image_jpeg BLOB, image_bytes)

windows_fts  -- FTS5 virtual table over (title, app_name, process_name, text)
```

WAL mode, `synchronous=NORMAL`, foreign keys ON. Snippets use
`snippet(windows_fts, 3, '«', '»', '…', 12)` so matched terms are highlighted in the UI.

---

## Security: encryption options

The database can be encrypted at rest with **SQLCipher** (AES-256 CBC + PBKDF2 + HMAC). You pick the key source in Settings:

| Mode | Where the key lives | Unlock UX | When to use |
|------|---------------------|-----------|-------------|
| **None** | — | No key | Sandbox machines, throwaway testing |
| **Windows account** | 32 random bytes generated once, **DPAPI-encrypted** with `DataProtectionScope.CurrentUser` at `%LOCALAPPDATA%\TotalRecall\db.key`. Decryptable **only** by your Windows user account on this machine. | Silent — auto-unlocks every launch | Default for personal machines |
| **Passphrase** | A passphrase you type. Never persisted. | Prompted each app launch (and required to start the MCP server) | Shared machines, second factor, USB-stick scenarios |

Switching encryption mode only affects newly-created databases. To re-encrypt an existing DB, point at a new file path and re-capture.

### Recommended hardening

- Run with **Windows account** encryption at minimum.
- Keep the DB on a **BitLocker**-encrypted drive.
- For shared machines, use **Passphrase** mode — even an admin can't unlock the file without your phrase.
- Use the built-in retention policy to strip old screenshots or delete old rows, then let automatic compaction run `VACUUM` to reclaim database file space.
- Never share the `db.key` file or the `.db` file. The `db.key` file is bound to your Windows user but is still a sensitive artifact.

### What is captured (privacy)

TotalRecall captures **whatever is visible on screen**. That includes:

- Email subjects and bodies, chat messages, document text.
- Anything visible in browser tabs, including passwords if they are not masked.
- Notification banners, calendar previews, etc.

Treat the database accordingly. There is no allow-list / block-list yet for which apps to skip — if you don't want a specific app captured, minimize it before the next tick or stop the capture.

---

## Build from source

> Most users should just grab the [release ZIP](#install) — this section is for contributors.

Prerequisites: **.NET 10 SDK** (preview or later), Windows 10/11 x64.

```powershell
git clone https://github.com/ilyafainberg/TotalRecall.git
cd TotalRecall
dotnet build TotalRecall.sln -c Release
```

To produce the same self-contained `.zip` that ships in releases:

```powershell
.\build-release.ps1 -Version 1.0.0
```

Outputs:

```
TotalRecall\bin\x64\Release\net10.0-windows\TotalRecall.exe        # UI
TotalRecall.Mcp\bin\x64\Release\net10.0-windows\TotalRecall.Mcp.exe # MCP server
```

Run the UI:

```powershell
.\TotalRecall\bin\x64\Release\net10.0-windows\TotalRecall.exe
```

The UI has three tabs:

- **Capture** — Start / Stop / Capture-now, live activity log, open-DB-folder button.
- **Browse** — FTS5 search box, app filter, date-range, results grid, JPEG preview + full OCR text on the right.
- **Settings** — capture interval, JPEG quality slider, store-JPEGs toggle, performance filters, DB path, OCR language, encryption mode + passphrase entry, retention + automatic compaction, **Start at login**, **Minimize to tray**.

Settings live in `%LOCALAPPDATA%\TotalRecall\settings.json`. The default database path is `%LOCALAPPDATA%\TotalRecall\recall.db`.

### Running in the background

Two settings under **Settings → Behavior** turn TotalRecall into a quiet background indexer:

- **Start TotalRecall when I sign in to Windows** — writes (or removes) a per-user entry under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` so the app launches with `--minimized` at every login. No admin rights, no scheduled task, no service. Only affects the current Windows user.
- **Minimize to system tray** — when on, the **X** button and the minimize button send the app to a tray icon instead of closing it. Right-click the tray icon for *Open / Start capture / Stop capture / Capture now / Exit*. Double-click to restore the window.

When the app is launched with `--minimized` **and** tray mode is on, it starts hidden in the tray and **auto-starts capture** (so a fresh login immediately resumes indexing). With `--minimized` but tray mode off, it just opens minimized to the taskbar.

You can also start it from any shortcut or scheduled task with the same flag:

```powershell
TotalRecall.exe --minimized
```

### Adding OCR languages

Drop additional `.traineddata` files from <https://github.com/tesseract-ocr/tessdata_fast> into `TotalRecall.Core\tessdata\` and rebuild. Then set the language in Settings (e.g. `eng+fra` for English + French).

---

## MCP server setup

The MCP server is a separate console executable that speaks JSON-RPC over stdio. It opens the same SQLite database the UI writes to and exposes 6 tools for an AI agent to query your activity history.

### Tools exposed

| Tool | Purpose |
|------|---------|
| `search_recall(query, app?, from?, to?, limit?)` | FTS5 search; returns windows with highlighted snippet |
| `get_window(window_id, include_image?)` | Full window row including OCR text; optional base64 JPEG |
| `list_snapshots(from?, to?, limit?)` | Tick-level browse |
| `get_snapshot(snapshot_id)` | All windows from a single tick |
| `list_apps()` | Distinct `app_name` values that have been captured |
| `stats()` | Row counts, total image bytes, first/last timestamps |

### Configuration

The MCP server reads `%LOCALAPPDATA%\TotalRecall\settings.json` to find the DB path, OCR language, and encryption mode — so once you've configured the UI you usually don't need to pass anything. You can override:

| Flag / env var | Purpose |
|---|---|
| `--db <path>` / `TOTALRECALL_DB` | Override the database path |
| `--passphrase <s>` / `TOTALRECALL_PASSPHRASE` | Supply a passphrase (required if you use passphrase encryption) |

### Registering with an AI assistant

#### Microsoft Scout / GitHub Copilot CLI

Edit `%USERPROFILE%\.copilot\mcp-config.json`:

```jsonc
{
  "mcpServers": {
    "TotalRecall": {
      "type": "local",
      "command": "C:\\Tools\\TotalRecall\\TotalRecall.Mcp\\TotalRecall.Mcp.exe",
      "args": [],
      "tools": ["*"]
    }
  }
}
```

Restart the agent for the new server to be picked up.

#### Claude Desktop

Edit `%APPDATA%\Claude\claude_desktop_config.json`:

```jsonc
{
  "mcpServers": {
    "totalrecall": {
      "command": "C:\\Tools\\TotalRecall\\TotalRecall.Mcp\\TotalRecall.Mcp.exe",
      "args": [],
      "env": {
        // Only needed if you chose passphrase encryption:
        // "TOTALRECALL_PASSPHRASE": "your phrase"
      }
    }
  }
}
```

Restart Claude Desktop.

#### Any other MCP-aware client

Spawn `TotalRecall.Mcp.exe` with stdio. Standard MCP `initialize` / `tools/list` / `tools/call` JSON-RPC over stdin/stdout.

### Once registered, you can ask things like

- *"Search my recall for any window mentioning 'invoice' in the last 3 hours."*
- *"What apps did I have open this morning?"*
- *"Find the window where I was reading about Power Platform pricing and summarize the text."*
- *"Show me the screenshot from window 4218."* (the agent will call `get_window` with `include_image: true`)

The agent only ever sees the rows it explicitly queries — it cannot stream or exfiltrate the whole DB.

---

## Roadmap / what's missing

- App allow-list / block-list (e.g. always skip 1Password, banking sites, etc.)
- Vector embedding of OCR text for semantic search alongside FTS5
- A "redact" pass on capture that masks anything that looks like a credential field
- Image-content embedding (CLIP / SigLIP) so the agent can do *visual* search ("the chart with the red bar")
- Export / share-out tools (encrypted snapshot archive, redacted JSON export)

PRs welcome.

---

## License

TotalRecall — a local screen-activity indexer.
Copyright © 2026 Ilya Fainberg.

This program is free software: you can redistribute it and/or modify it under the terms of the **GNU General Public License** as published by the Free Software Foundation, either **version 3** of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but **WITHOUT ANY WARRANTY**; without even the implied warranty of **MERCHANTABILITY** or **FITNESS FOR A PARTICULAR PURPOSE**. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program in the file [LICENSE](LICENSE). If not, see <https://www.gnu.org/licenses/>.
