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

public enum EncryptionMode
{
    None = 0,
    UserAccount = 1, // DPAPI-protected random key
    Passphrase = 2,  // user-supplied passphrase
}

public sealed class AppSettings
{
    public int IntervalSeconds { get; set; } = 10;
    public int JpegQuality { get; set; } = 75;     // 30..95
    public bool StoreScreenshots { get; set; } = true;
    public string DatabasePath { get; set; } = DefaultDbPath();
    public EncryptionMode EncryptionMode { get; set; } = EncryptionMode.None;
    public string OcrLanguage { get; set; } = "eng";
    public bool EnableChangeDetection { get; set; } = true;
    public bool CaptureForegroundOnly { get; set; } = false;
    public string ExcludedApps { get; set; } = "";
    public int OcrMaxDimension { get; set; } = 1600;

    public bool StartAtLogin { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;

    // Remembered capture state: true if the user had capture running last time they
    // closed the app. The launcher will resume capture automatically on next start.
    public bool WasCapturing { get; set; } = false;

    // --- Retention -----------------------------------------------------
    // Delete only the JPEG blob from rows older than N days (keeps OCR text + metadata).
    public bool PurgeImagesEnabled { get; set; } = false;
    public int  PurgeImagesAfterDays { get; set; } = 30;

    // Delete rows (images + OCR text + metadata) older than N days.
    public bool PurgeAllEnabled { get; set; } = false;
    public int  PurgeAllAfterDays { get; set; } = 90;

    // Reclaim database file space after retention removes rows or image blobs.
    public bool CompactAfterRetentionEnabled { get; set; } = true;
    public int CompactAfterRetentionHours { get; set; } = 24;
    public string? LastCompactionUtc { get; set; }

    [JsonIgnore]
    public string? RuntimePassphrase { get; set; } // never serialized

    public static string AppDataDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TotalRecall");

    public static string SettingsPath => Path.Combine(AppDataDir, "settings.json");

    public static string DefaultDbPath() => Path.Combine(AppDataDir, "recall.db");

    public static string KeyFilePath => Path.Combine(AppDataDir, "db.key");

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
        catch { /* fall through to defaults */ }
        return new AppSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(AppDataDir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}
