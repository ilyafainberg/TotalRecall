# TotalRecall MCP Server

This bundle is the **standalone Model Context Protocol (MCP) server** for
[TotalRecall](https://github.com/ilyafainberg/TotalRecall) — a local-first
Windows screen indexer that captures windows, OCRs them, and stores searchable
text in an encrypted SQLite database.

The MCP server lets AI agents query that index over the standard MCP `stdio`
transport. It is self-contained: the bundled `.exe` is a .NET 10 ahead-of-time
publish, no .NET runtime install required.

> You can use this MCP server **without** installing the full TotalRecall
> desktop app — as long as a TotalRecall database already exists somewhere on
> disk (created by the desktop app, or by any other tool that writes the same
> schema). The server is read-only by default.

---

## What's in this bundle

```
TotalRecall.Mcp\
  TotalRecall.Mcp.exe        ← the server binary (stdio MCP)
  *.dll                      ← .NET 10 + dependencies (self-contained)
  tessdata\                  ← Tesseract OCR language data
mcp.config.sample.json       ← drop-in config snippet
LICENSE
README.md                    ← this file
```

---

## Tools exposed

| Tool                    | What it does                                                   |
|-------------------------|----------------------------------------------------------------|
| `search_index`          | Full-text search across captured window text (FTS5).           |
| `list_recent_captures`  | List the most recent capture rows (paged).                     |
| `get_capture`           | Fetch a single capture (window list + per-window text) by id.  |
| `summarize_day`         | Group captures by app + window title for a given date.         |
| `database_info`         | Path, size, row counts, encryption status.                     |

Run the .exe with `--help` for the full list and current schema:

```powershell
.\TotalRecall.Mcp\TotalRecall.Mcp.exe --help
```

---

## Configure an MCP host

### Microsoft Scout / Clawpilot

Edit `%USERPROFILE%\.clawpilot\mcp-config.json` (or whichever
`mcp-config.json` your install uses). Add an entry under `mcpServers`:

```json
{
  "mcpServers": {
    "totalrecall": {
      "command": "C:\\Tools\\TotalRecall.Mcp\\TotalRecall.Mcp.exe",
      "args": [],
      "env": {
        "TOTALRECALL_DB_PATH": "C:\\Users\\<you>\\AppData\\Local\\TotalRecall\\totalrecall.db"
      }
    }
  }
}
```

Restart Scout. The server should appear green in Settings → Extensions → MCP.

### GitHub Copilot CLI

Add the same entry to `%USERPROFILE%\.copilot\mcp-config.json`:

```json
{
  "mcpServers": {
    "totalrecall": {
      "command": "C:\\Tools\\TotalRecall.Mcp\\TotalRecall.Mcp.exe",
      "args": [],
      "env": {
        "TOTALRECALL_DB_PATH": "C:\\Users\\<you>\\AppData\\Local\\TotalRecall\\totalrecall.db"
      }
    }
  }
}
```

Run `copilot mcp list` to verify it shows up; then `copilot mcp test totalrecall`
to round-trip a tool call.

### Claude Desktop

`%APPDATA%\Claude\claude_desktop_config.json`, same shape:

```json
{
  "mcpServers": {
    "totalrecall": {
      "command": "C:\\Tools\\TotalRecall.Mcp\\TotalRecall.Mcp.exe",
      "args": []
    }
  }
}
```

### VS Code (Copilot Chat)

`%APPDATA%\Code\User\settings.json`:

```jsonc
{
  "github.copilot.chat.mcp.servers": {
    "totalrecall": {
      "command": "C:\\Tools\\TotalRecall.Mcp\\TotalRecall.Mcp.exe",
      "args": []
    }
  }
}
```

---

## Environment variables

| Variable                  | Purpose                                                            |
|---------------------------|--------------------------------------------------------------------|
| `TOTALRECALL_DB_PATH`     | Override path to `totalrecall.db`. Default: `%LOCALAPPDATA%\TotalRecall\totalrecall.db` |
| `TOTALRECALL_DB_KEY`      | SQLCipher key. Required if your DB was created encrypted.          |
| `TOTALRECALL_READONLY`    | `1` to forbid any write tool (default).                            |
| `TOTALRECALL_LOG_LEVEL`   | `Trace` / `Debug` / `Information` / `Warning` / `Error`.           |

---

## Verifying the server

From any terminal:

```powershell
echo '{"jsonrpc":"2.0","id":1,"method":"tools/list"}' | .\TotalRecall.Mcp\TotalRecall.Mcp.exe
```

You should see a JSON response listing every tool above. If you get nothing,
re-run with `TOTALRECALL_LOG_LEVEL=Debug` and check stderr.

---

## License

MIT — see `LICENSE`.
