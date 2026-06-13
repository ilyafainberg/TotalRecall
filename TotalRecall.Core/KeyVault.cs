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
using System.Security.Cryptography;

namespace TotalRecall;

/// <summary>
/// Manages the SQLCipher database key:
///   - None        => no key
///   - UserAccount => random 32-byte key stored encrypted via DPAPI (CurrentUser) at <AppData>\TotalRecall\db.key
///   - Passphrase  => caller-supplied passphrase
/// </summary>
public static class KeyVault
{
    public static string? GetKey(AppSettings settings)
    {
        return settings.EncryptionMode switch
        {
            EncryptionMode.None        => null,
            EncryptionMode.UserAccount => GetOrCreateUserKey(),
            EncryptionMode.Passphrase  => settings.RuntimePassphrase
                ?? throw new InvalidOperationException("Passphrase encryption is enabled but no passphrase was provided."),
            _ => null,
        };
    }

    /// <summary>
    /// Returns a 64-char hex string suitable for SQLCipher's `PRAGMA key = "x'...'"` form
    /// (Microsoft.Data.Sqlite passes the Password connection-string field as the key).
    /// </summary>
    private static string GetOrCreateUserKey()
    {
        Directory.CreateDirectory(AppSettings.AppDataDir);
        var path = AppSettings.KeyFilePath;
        byte[] keyBytes;

        if (File.Exists(path))
        {
            var encrypted = File.ReadAllBytes(path);
            keyBytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        }
        else
        {
            keyBytes = RandomNumberGenerator.GetBytes(32);
            var encrypted = ProtectedData.Protect(keyBytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(path, encrypted);
        }

        // SQLCipher accepts a passphrase OR a raw key. We use a passphrase form (hex string)
        // for simplicity; SQLCipher will run PBKDF2 over it. Acceptable for a local app.
        return Convert.ToHexString(keyBytes);
    }

    public static void ResetUserKey()
    {
        if (File.Exists(AppSettings.KeyFilePath))
            File.Delete(AppSettings.KeyFilePath);
    }
}
