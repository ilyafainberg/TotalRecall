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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TotalRecall;

/// <summary>
/// How the SQLCipher database key is sourced.
/// </summary>
/// <remarks>
/// <see cref="None"/>: plaintext DB (no encryption — fastest, but anyone with file
/// access can read your captures).
/// <see cref="UserAccount"/>: random 32-byte key generated once, wrapped with DPAPI
/// (CurrentUser scope), stored as <c>%LocalAppData%\TotalRecall\db.key</c>. Only the
/// same Windows user account on the same machine can decrypt it.
/// <see cref="Passphrase"/>: user-supplied passphrase, never stored; must be re-entered
/// after restart and held in <see cref="AppSettings.RuntimePassphrase"/> in process.
/// </remarks>
public enum EncryptionMode
{
    None = 0,
    UserAccount = 1, // DPAPI-protected random key
    Passphrase = 2,  // user-supplied passphrase
}

/// <summary>
/// JSON-serialised user settings — the single source of truth for both the WinForms app
/// and the standalone MCP server. Stored at <c>%LocalAppData%\TotalRecall\settings.json</c>.
/// </summary>
/// <remarks>
/// Add a new setting by:
///   1. Adding a property with a sensible default (back-compat: missing JSON properties
///      will use that default).
///   2. Surfacing it in <see cref="SettingsPanel"/> (designer + BindSettings / Save).
///   3. Wiring it through to whichever service consumes it (usually
///      <see cref="SnapshotService"/> or <see cref="OcrService"/>).
/// Do NOT add a constructor that runs validation — <see cref="JsonSerializer"/> uses the
/// parameterless ctor and assigns properties one at a time.
/// </remarks>
public sealed class AppSettings
{
    /// <summary>Seconds between capture ticks. UI clamps to 1..3600.</summary>
    public int IntervalSeconds { get; set; } = 10;

    /// <summary>JPEG quality (30..95). Higher = larger blobs and slower disk I/O.</summary>
    public int JpegQuality { get; set; } = 75;     // 30..95

    /// <summary>If false, OCR still runs but the JPEG blob is dropped after extraction.</summary>
    public bool StoreScreenshots { get; set; } = true;

    public string DatabasePath { get; set; } = DefaultDbPath();
    public EncryptionMode EncryptionMode { get; set; } = EncryptionMode.None;

    /// <summary>Tesseract language identifier (matches the *.traineddata filename in <c>tessdata\</c>).</summary>
    public string OcrLanguage { get; set; } = "eng";

    /// <summary>When true, the capture loop fingerprints each window and skips unchanged frames.</summary>
    public bool EnableChangeDetection { get; set; } = true;

    /// <summary>When true, only the currently-focused window is captured each tick.</summary>
    public bool CaptureForegroundOnly { get; set; } = false;

    /// <summary>
    /// Comma / semicolon / newline-separated tokens. A window is excluded if any token
    /// is a substring (case-insensitive) of its app name, process name, title, or exe path.
    /// </summary>
    public string ExcludedApps { get; set; } = "";

    /// <summary>Longest side (in pixels) before OCR pre-processing downscales the bitmap. Clamped 400..3840.</summary>
    public int OcrMaxDimension { get; set; } = 1600;

    /// <summary>Register/unregister a per-user HKCU Run entry on save.</summary>
    public bool StartAtLogin { get; set; } = false;

    /// <summary>When true, X-button and Minimize hide the window into the tray instead of closing.</summary>
    public bool MinimizeToTray { get; set; } = false;

    /// <summary>
    /// Remembered capture state: true if the user had capture running last time they
    /// closed the app. The launcher resumes capture automatically on next start unless
    /// overridden via <c>--capture-on / --capture-off</c>.
    /// </summary>
    public bool WasCapturing { get; set; } = false;

    // --- Retention -----------------------------------------------------
    // Two-stage retention: a row is first stripped of its image blob (cheaper to keep
    // around for search), and later the whole row is deleted entirely. Either stage
    // can be disabled independently.

    /// <summary>Delete only the JPEG blob from rows older than <see cref="PurgeImagesAfterDays"/>.</summary>
    public bool PurgeImagesEnabled { get; set; } = false;
    public int  PurgeImagesAfterDays { get; set; } = 30;

    /// <summary>Delete entire rows (image + OCR text + metadata) older than <see cref="PurgeAllAfterDays"/>.</summary>
    public bool PurgeAllEnabled { get; set; } = false;
    public int  PurgeAllAfterDays { get; set; } = 90;

    /// <summary>Reclaim database file space (VACUUM) after retention removes rows or image blobs.</summary>
    public bool CompactAfterRetentionEnabled { get; set; } = true;

    /// <summary>Minimum hours between automatic compactions. <c>0</c> means run on every retention pass.</summary>
    public int CompactAfterRetentionHours { get; set; } = 24;

    /// <summary>ISO-8601 timestamp of the last successful VACUUM. Set by the runtime; do not edit by hand.</summary>
    public string? LastCompactionUtc { get; set; }

    /// <summary>
    /// In-memory only: the user's passphrase when <see cref="EncryptionMode"/> is
    /// <see cref="EncryptionMode.Passphrase"/>. <see cref="JsonIgnore"/> ensures it never hits disk.
    /// </summary>
    [JsonIgnore]
    public string? RuntimePassphrase { get; set; }

    public static string AppDataDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TotalRecall");

    public static string SettingsPath => Path.Combine(AppDataDir, "settings.json");

    public static string DefaultDbPath() => Path.Combine(AppDataDir, "recall.db");

    /// <summary>Path to the DPAPI-wrapped random key when <see cref="EncryptionMode.UserAccount"/> is in use.</summary>
    public static string KeyFilePath => Path.Combine(AppDataDir, "db.key");

    /// <summary>Loads settings from disk, or returns a fresh default instance on any failure (missing/corrupt JSON).</summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { /* fall through to defaults — a busted file shouldn't prevent the app from launching */ }
        return new AppSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(AppDataDir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}
