# One-shot agent install prompt

Paste this verbatim into an AI agent that has shell + file-edit tools
(Microsoft Scout, GitHub Copilot CLI, Claude Code, Cursor, etc.).

> Stop the agent before pasting and review the prompt — it downloads,
> extracts, and writes config files on your behalf.

---

```
You're going to install the TotalRecall MCP server on this Windows machine.
Do all of the following without asking for confirmation between steps:

1. Download the latest TotalRecall MCP zip from the GitHub release:
     https://github.com/ilyafainberg/TotalRecall/releases/latest
   The asset name pattern is:  TotalRecall-Mcp-<version>-win-x64.zip
   Use the GitHub API (https://api.github.com/repos/ilyafainberg/TotalRecall/releases/latest)
   if you need to resolve the exact URL. Save it to %TEMP%.

2. Extract the zip into a stable per-user install dir:
     %LOCALAPPDATA%\Programs\TotalRecall.Mcp\
   Overwrite if it already exists. After extraction the server binary must be at:
     %LOCALAPPDATA%\Programs\TotalRecall.Mcp\TotalRecall.Mcp\TotalRecall.Mcp.exe

3. Sanity-check the server: run the .exe once with `--help` and confirm it
   prints a tools list without crashing. If it crashes, surface the stderr
   to me and stop.

4. Detect which MCP host I'm running on (you are the agent — pick the right one):

   - Microsoft Scout / Clawpilot →  %USERPROFILE%\.clawpilot\mcp-config.json
   - GitHub Copilot CLI          →  %USERPROFILE%\.copilot\mcp-config.json
   - Claude Desktop              →  %APPDATA%\Claude\claude_desktop_config.json
   - VS Code Copilot Chat        →  %APPDATA%\Code\User\settings.json
                                    (key: github.copilot.chat.mcp.servers)
   - Cursor                      →  %USERPROFILE%\.cursor\mcp.json

   Read the existing file (or create it if missing) and merge — DO NOT
   replace — an entry under the appropriate "mcpServers" key:

     "totalrecall": {
       "command": "<full path to the .exe you installed in step 2>",
       "args": [],
       "env": {
         "TOTALRECALL_DB_PATH": "<%LOCALAPPDATA%>\\TotalRecall\\totalrecall.db"
       }
     }

   Preserve any other mcpServers already in the file. Pretty-print the JSON.

5. Tell me:
   - which host you configured
   - the absolute path to the installed .exe
   - whether the TotalRecall DB at TOTALRECALL_DB_PATH exists yet, and if not,
     remind me to install the TotalRecall desktop app from the same release
     page and run it at least once to create the DB.
   - that I need to restart the host before the server shows up.

If anything goes wrong, stop and tell me what failed. Don't keep going past
the first error.
```

---

## Why it's safe to paste

- Only writes to per-user dirs (`%LOCALAPPDATA%`, `%USERPROFILE%`,
  `%APPDATA%`) — no admin rights, no system-wide changes.
- Merges into existing `mcp-config.json` files, doesn't overwrite.
- Self-contained .NET 10 binary, no runtime install.
- Server is read-only by default (writes are gated by
  `TOTALRECALL_READONLY=0`).
- Source: https://github.com/ilyafainberg/TotalRecall
