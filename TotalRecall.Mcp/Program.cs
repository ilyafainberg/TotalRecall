// TotalRecall — a local screen-activity indexer.
// Copyright (C) 2026 Ilya Fainberg.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using TotalRecall;

namespace TotalRecall.Mcp;

/// <summary>
/// Entry point for the TotalRecall MCP (Model Context Protocol) server. Hosts the
/// <see cref="RecallTools"/> tool surface over stdio so any MCP-capable agent
/// (Claude Desktop, Copilot CLI, custom clients) can query the recall database.
/// </summary>
/// <remarks>
/// <para>Designed to be launched by an MCP client — never interactively. All output
/// except for MCP protocol traffic on stdout/stderr is logged to a rotating file in
/// <c>%LOCALAPPDATA%\TotalRecall\mcp-logs\</c>.</para>
/// <para>Database resolution order (first hit wins):</para>
/// <list type="number">
///   <item><c>--db &lt;path&gt;</c> CLI arg</item>
///   <item><c>TOTALRECALL_DB</c> environment variable</item>
///   <item><c>AppSettings.Load().DbPath</c> from the user's settings.json</item>
/// </list>
/// <para>Same precedence applies for encryption passphrase
/// (<c>--passphrase</c> / <c>TOTALRECALL_PASSPHRASE</c> / settings).</para>
/// </remarks>
internal static class Program
{
    public static async Task Main(string[] args)
    {
        // Resolve which DB to open. Order of precedence:
        //   1. --db <path>            CLI override
        //   2. TOTALRECALL_DB env var
        //   3. settings.json (AppSettings.Load)
        //
        // Encryption mode + passphrase likewise:
        //   --passphrase <s>  /  TOTALRECALL_PASSPHRASE  /  settings.json

        var settings = AppSettings.Load();
        string? dbOverride = GetArg(args, "--db") ?? Environment.GetEnvironmentVariable("TOTALRECALL_DB");
        if (!string.IsNullOrWhiteSpace(dbOverride)) settings.DatabasePath = dbOverride;

        string? passOverride = GetArg(args, "--passphrase") ?? Environment.GetEnvironmentVariable("TOTALRECALL_PASSPHRASE");
        if (!string.IsNullOrWhiteSpace(passOverride))
        {
            settings.EncryptionMode = EncryptionMode.Passphrase;
            settings.RuntimePassphrase = passOverride;
        }

        if (!File.Exists(settings.DatabasePath))
        {
            await Console.Error.WriteLineAsync(
                $"[TotalRecall.Mcp] Warning: database not found at {settings.DatabasePath}. " +
                "Tools will return zero results until the WinForms app creates it.");
        }

        string? key;
        try { key = KeyVault.GetKey(settings); }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync("[TotalRecall.Mcp] Cannot resolve DB key: " + ex.Message);
            Environment.ExitCode = 2;
            return;
        }

        // Pre-open once so we fail fast on bad key.
        var db = new Database(settings.DatabasePath, key);
        try { db.VerifyAccessible(); }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync(
                "[TotalRecall.Mcp] Could not open DB with configured key: " + ex.Message);
            Environment.ExitCode = 3;
            return;
        }

        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(opts => opts.LogToStandardErrorThreshold = LogLevel.Trace);

        builder.Services.AddSingleton(settings);
        builder.Services.AddSingleton(db);

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        var host = builder.Build();
        await host.RunAsync();
    }

    private static string? GetArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }
}
