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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;

namespace TotalRecall;

/// <summary>
/// SQLCipher-backed persistence for snapshots, windows, and full-text search.
/// </summary>
/// <remarks>
/// <para>Schema (see <c>EnsureSchema</c>):</para>
/// <list type="bullet">
///   <item><c>snapshots(id, captured_at_utc, user_name)</c> — one row per capture tick.</item>
///   <item><c>windows(id, snapshot_id, title, app_name, …, image_bytes, text)</c>
///     — one row per stored window. The JPEG payload no longer lives here: new captures
///     write it to <c>window_images</c> and leave the legacy <c>image_jpeg</c> column NULL.
///     <c>image_bytes</c> is retained as cheap size metadata.</item>
///   <item><c>window_images(window_id, jpeg)</c> — 1:1 side table holding the JPEG blob,
///     <c>ON DELETE CASCADE</c> from <c>windows</c>. Splitting the blobs out keeps the
///     <c>windows</c> b-tree small so metadata/search scans stay fast on large databases.
///     Reads fall back to the legacy inline <c>image_jpeg</c> for pre-split rows.</item>
///   <item><c>windows_fts</c> — external-content FTS5 virtual table mirroring
///     <c>title</c>, <c>app_name</c>, <c>text_content</c>. Three triggers keep it in sync
///     on INSERT/UPDATE/DELETE so searches stay correct after retention purges.</item>
/// </list>
/// <para><b>Encryption:</b> SQLCipher key is supplied via the <c>Password=</c> field on the
/// connection string and set via <c>PRAGMA key</c> on every connection open. When the user
/// chooses "No encryption" we connect without a key (plain SQLite). The provider package
/// (<c>SQLitePCLRaw.bundle_e_sqlcipher</c>) replaces the default SQLite native lib with one
/// that has SQLCipher compiled in — no other code changes required for either mode.</para>
/// <para><b>Concurrency:</b> Microsoft.Data.Sqlite is connection-per-operation; the
/// underlying file is opened with <c>journal_mode=WAL</c> so reads from the Browse pane
/// don't block writes from the capture loop.</para>
/// </remarks>
public sealed class Database : IDisposable
{
    private static int initialized;
    private readonly string connectionString;

    public string DatabasePath { get; }

    static Database()
    {
        // Wire up the SQLCipher provider.
        if (System.Threading.Interlocked.Exchange(ref initialized, 1) == 0)
        {
            SQLitePCL.Batteries_V2.Init();
        }
    }

    public Database(string path, string? key)
    {
        DatabasePath = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        var csb = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true,
        };
        if (!string.IsNullOrEmpty(key))
            csb.Password = key;

        connectionString = csb.ConnectionString;
        EnsureSchema();
    }

    public SqliteConnection Open()
    {
        var conn = new SqliteConnection(connectionString);
        conn.Open();
        using (var pragma = conn.CreateCommand())
        {
            // temp_store=MEMORY keeps ORDER BY/GROUP BY scratch off disk; a larger page
            // cache (64 MB) dramatically cuts repeated reads on a large DB. (mmap is a
            // no-op under SQLCipher, so we don't bother setting it.)
            pragma.CommandText =
                "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA foreign_keys=ON; " +
                "PRAGMA temp_store=MEMORY; PRAGMA cache_size=-65536;";
            pragma.ExecuteNonQuery();
        }
        return conn;
    }

    private void EnsureSchema()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS meta (
                key TEXT PRIMARY KEY,
                value TEXT
            );

            CREATE TABLE IF NOT EXISTS snapshots (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                ts              TEXT    NOT NULL,
                ts_unix         INTEGER NOT NULL,
                user            TEXT    NOT NULL,
                machine         TEXT    NOT NULL,
                window_count    INTEGER NOT NULL,
                elapsed_ms      INTEGER NOT NULL DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS idx_snapshots_ts ON snapshots(ts_unix DESC);

            CREATE TABLE IF NOT EXISTS windows (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                snapshot_id     INTEGER NOT NULL REFERENCES snapshots(id) ON DELETE CASCADE,
                title           TEXT,
                app_name        TEXT,
                process_name    TEXT,
                process_id      INTEGER,
                executable_path TEXT,
                is_foreground   INTEGER NOT NULL DEFAULT 0,
                bounds_x        INTEGER, bounds_y INTEGER, bounds_w INTEGER, bounds_h INTEGER,
                text            TEXT,
                ocr_error       TEXT,
                ocr_duration_ms INTEGER NOT NULL DEFAULT 0,
                image_jpeg      BLOB,
                image_bytes     INTEGER NOT NULL DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS idx_windows_snapshot ON windows(snapshot_id);
            CREATE INDEX IF NOT EXISTS idx_windows_app     ON windows(app_name);

            -- JPEG payloads live in a side table keyed 1:1 on the window row. Keeping the
            -- (large) blobs out of the `windows` table keeps metadata/search scans fast and
            -- the main b-tree small. `image_bytes` stays on `windows` as cheap metadata.
            -- ON DELETE CASCADE means deleting a window row drops its image automatically.
            CREATE TABLE IF NOT EXISTS window_images (
                window_id   INTEGER PRIMARY KEY REFERENCES windows(id) ON DELETE CASCADE,
                jpeg        BLOB NOT NULL
            );

            CREATE VIRTUAL TABLE IF NOT EXISTS windows_fts USING fts5(
                title, app_name, process_name, text,
                content='windows', content_rowid='id',
                tokenize='unicode61 remove_diacritics 2'
            );

            CREATE TRIGGER IF NOT EXISTS windows_ai AFTER INSERT ON windows BEGIN
                INSERT INTO windows_fts(rowid, title, app_name, process_name, text)
                VALUES (new.id, new.title, new.app_name, new.process_name, new.text);
            END;
            CREATE TRIGGER IF NOT EXISTS windows_ad AFTER DELETE ON windows BEGIN
                INSERT INTO windows_fts(windows_fts, rowid, title, app_name, process_name, text)
                VALUES('delete', old.id, old.title, old.app_name, old.process_name, old.text);
            END;
            CREATE TRIGGER IF NOT EXISTS windows_au AFTER UPDATE ON windows BEGIN
                INSERT INTO windows_fts(windows_fts, rowid, title, app_name, process_name, text)
                VALUES('delete', old.id, old.title, old.app_name, old.process_name, old.text);
                INSERT INTO windows_fts(rowid, title, app_name, process_name, text)
                VALUES (new.id, new.title, new.app_name, new.process_name, new.text);
            END;

            INSERT OR IGNORE INTO meta(key, value) VALUES('schema_version', '1');
            """;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Probes that the key works by running a trivial query.
    /// Throws SqliteException on bad key.
    /// </summary>
    public void VerifyAccessible()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT count(*) FROM sqlite_master;";
        cmd.ExecuteScalar();
    }

    public DatabaseStats GetStats()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT (SELECT COUNT(*) FROM snapshots),
                   (SELECT COUNT(*) FROM windows),
                   (SELECT COALESCE(SUM(image_bytes), 0) FROM windows),
                   (SELECT MIN(ts) FROM snapshots),
                   (SELECT MAX(ts) FROM snapshots);
            """;
        using var rdr = cmd.ExecuteReader();
        rdr.Read();
        return new DatabaseStats(
            Snapshots: rdr.GetInt64(0),
            Windows: rdr.GetInt64(1),
            ImageBytes: rdr.GetInt64(2),
            FirstTimestamp: rdr.IsDBNull(3) ? null : rdr.GetString(3),
            LastTimestamp: rdr.IsDBNull(4) ? null : rdr.GetString(4));
    }

    public List<string> GetDistinctAppNames()
    {
        var list = new List<string>();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT DISTINCT app_name FROM windows WHERE app_name IS NOT NULL AND app_name <> '' ORDER BY app_name COLLATE NOCASE;";
        using var rdr = cmd.ExecuteReader();
        while (rdr.Read()) list.Add(rdr.GetString(0));
        return list;
    }

    /// <summary>
    /// Wipes every row from `windows` and `snapshots`. FTS5 entries are removed by the
    /// existing AFTER DELETE trigger on `windows`. Returns the number of snapshot rows deleted.
    /// </summary>
    public int ClearAll()
    {
        using var conn = Open();
        int snapshotCount;
        using (var tx = conn.BeginTransaction())
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM window_images;";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM windows;";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM snapshots;";
                snapshotCount = cmd.ExecuteNonQuery();
            }
            tx.Commit();
        }
        return snapshotCount;
    }

    /// <summary>
    /// Strips the JPEG blob from rows whose snapshot is older than <paramref name="days"/>.
    /// OCR text and metadata are preserved so search still works. Returns number of rows updated.
    /// </summary>
    public int PurgeImagesOlderThan(int days)
    {
        if (days <= 0) return 0;
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days).ToUnixTimeSeconds();
        using var conn = Open();
        using var tx = conn.BeginTransaction();
        int affected;
        using (var del = conn.CreateCommand())
        {
            del.Transaction = tx;
            // Drop the side-table blobs for old rows.
            del.CommandText = """
                DELETE FROM window_images
                 WHERE window_id IN (
                     SELECT w.id FROM windows w
                      WHERE w.snapshot_id IN (SELECT id FROM snapshots WHERE ts_unix < $c));
                """;
            del.Parameters.AddWithValue("$c", cutoff);
            del.ExecuteNonQuery();
        }
        using (var upd = conn.CreateCommand())
        {
            upd.Transaction = tx;
            // Clear any legacy inline blobs and zero the metadata so size sums stay honest.
            upd.CommandText = """
                UPDATE windows
                   SET image_jpeg = NULL, image_bytes = 0
                 WHERE image_bytes > 0
                   AND snapshot_id IN (SELECT id FROM snapshots WHERE ts_unix < $c);
                """;
            upd.Parameters.AddWithValue("$c", cutoff);
            affected = upd.ExecuteNonQuery();
        }
        tx.Commit();
        return affected;
    }

    /// <summary>
    /// Deletes rows (image + OCR text + metadata) for snapshots older than <paramref name="days"/>.
    /// FTS5 entries are removed by the existing AFTER DELETE trigger. Returns rows deleted from `windows`.
    /// </summary>
    public int PurgeRowsOlderThan(int days)
    {
        if (days <= 0) return 0;
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days).ToUnixTimeSeconds();
        using var conn = Open();
        using var tx = conn.BeginTransaction();
        int windowsDeleted;
        using (var del = conn.CreateCommand())
        {
            del.Transaction = tx;
            del.CommandText = "DELETE FROM windows WHERE snapshot_id IN (SELECT id FROM snapshots WHERE ts_unix < $c);";
            del.Parameters.AddWithValue("$c", cutoff);
            windowsDeleted = del.ExecuteNonQuery();
        }
        using (var del2 = conn.CreateCommand())
        {
            del2.Transaction = tx;
            del2.CommandText = "DELETE FROM snapshots WHERE ts_unix < $c;";
            del2.Parameters.AddWithValue("$c", cutoff);
            del2.ExecuteNonQuery();
        }
        tx.Commit();
        return windowsDeleted;
    }

    /// <summary>
    /// Applies the retention policy in the correct order: full-row purge first (drops
    /// everything past its cutoff), then image-only purge on whatever survives.
    /// </summary>
    public RetentionResult ApplyRetention(AppSettings settings)
    {
        int rows = 0, imgs = 0;
        if (settings.PurgeAllEnabled && settings.PurgeAllAfterDays > 0)
            rows = PurgeRowsOlderThan(settings.PurgeAllAfterDays);
        if (settings.PurgeImagesEnabled && settings.PurgeImagesAfterDays > 0)
            imgs = PurgeImagesOlderThan(settings.PurgeImagesAfterDays);
        return new RetentionResult(imgs, rows);
    }

    public long InsertSnapshot(DateTimeOffset ts, string user, string machine, int windowCount, long elapsedMs)
    {
        using var conn = Open();
        return InsertSnapshot(conn, null, ts, user, machine, windowCount, elapsedMs);
    }

    private static long InsertSnapshot(SqliteConnection conn, SqliteTransaction? tx,
        DateTimeOffset ts, string user, string machine, int windowCount, long elapsedMs)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO snapshots(ts, ts_unix, user, machine, window_count, elapsed_ms)
            VALUES($ts, $tsu, $u, $m, $wc, $e);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$ts", ts.ToString("o"));
        cmd.Parameters.AddWithValue("$tsu", ts.ToUnixTimeSeconds());
        cmd.Parameters.AddWithValue("$u", user);
        cmd.Parameters.AddWithValue("$m", machine);
        cmd.Parameters.AddWithValue("$wc", windowCount);
        cmd.Parameters.AddWithValue("$e", elapsedMs);
        return (long)cmd.ExecuteScalar()!;
    }

    public void InsertWindow(long snapshotId, WindowRecord r, byte[]? jpeg)
    {
        using var conn = Open();
        InsertWindow(conn, null, snapshotId, r, jpeg);
    }

    private static void InsertWindow(SqliteConnection conn, SqliteTransaction? tx,
        long snapshotId, WindowRecord r, byte[]? jpeg)
    {
        long windowId;
        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = tx;
            // New rows keep the JPEG out of the `windows` table entirely — the blob goes
            // into `window_images` below. `image_jpeg` stays in the schema only so older
            // databases (pre-split) remain readable; we never write to it anymore.
            cmd.CommandText = """
                INSERT INTO windows(snapshot_id, title, app_name, process_name, process_id, executable_path,
                                    is_foreground, bounds_x, bounds_y, bounds_w, bounds_h,
                                    text, ocr_error, ocr_duration_ms, image_jpeg, image_bytes)
                VALUES($s,$t,$a,$pn,$pi,$ep,$f,$bx,$by,$bw,$bh,$tx,$oe,$od,NULL,$ib);
                SELECT last_insert_rowid();
                """;
            cmd.Parameters.AddWithValue("$s", snapshotId);
            cmd.Parameters.AddWithValue("$t", (object?)r.Title ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$a", (object?)r.AppName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$pn", (object?)r.ProcessName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$pi", r.ProcessId);
            cmd.Parameters.AddWithValue("$ep", (object?)r.ExecutablePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$f", r.IsForeground ? 1 : 0);
            cmd.Parameters.AddWithValue("$bx", r.Bounds?.X ?? 0);
            cmd.Parameters.AddWithValue("$by", r.Bounds?.Y ?? 0);
            cmd.Parameters.AddWithValue("$bw", r.Bounds?.Width ?? 0);
            cmd.Parameters.AddWithValue("$bh", r.Bounds?.Height ?? 0);
            cmd.Parameters.AddWithValue("$tx", (object?)r.Text ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$oe", (object?)r.OcrError ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$od", r.OcrDurationMs);
            cmd.Parameters.AddWithValue("$ib", jpeg?.Length ?? 0);
            windowId = (long)cmd.ExecuteScalar()!;
        }

        if (jpeg is { Length: > 0 })
        {
            using var img = conn.CreateCommand();
            img.Transaction = tx;
            img.CommandText = "INSERT INTO window_images(window_id, jpeg) VALUES($id, $j);";
            img.Parameters.AddWithValue("$id", windowId);
            img.Parameters.AddWithValue("$j", jpeg);
            img.ExecuteNonQuery();
        }
    }

    public long InsertSnapshotWithWindows(DateTimeOffset ts, string user, string machine,
        int observedWindowCount, long elapsedMs, IReadOnlyList<CapturedWindow> windows)
    {
        using var conn = Open();
        using var tx = conn.BeginTransaction();
        var snapshotId = InsertSnapshot(conn, tx, ts, user, machine, observedWindowCount, elapsedMs);
        foreach (var window in windows)
            InsertWindow(conn, tx, snapshotId, window.Record, window.Jpeg);
        tx.Commit();
        return snapshotId;
    }

    /// <summary>Search windows. Empty query returns most recent. Filters are AND-combined.</summary>
    public List<SearchHit> Search(string? query, string? appFilter,
        DateTimeOffset? from, DateTimeOffset? to, int limit = 200)
    {
        var hits = new List<SearchHit>();
        using var conn = Open();
        using var cmd = conn.CreateCommand();

        var matchQuery = string.IsNullOrWhiteSpace(query) ? "" : BuildFtsMatchQuery(query);
        var hasQuery = matchQuery.Length > 0;
        var sql = hasQuery ? """
            SELECT w.id, w.snapshot_id, s.ts, s.user, s.machine,
                   w.title, w.app_name, w.process_name, w.process_id, w.is_foreground,
                   snippet(windows_fts, 3, '«', '»', '…', 12) AS snip,
                   length(COALESCE(w.text,'')) AS tlen, w.image_bytes
              FROM windows_fts
              JOIN windows  w ON w.id = windows_fts.rowid
              JOIN snapshots s ON s.id = w.snapshot_id
             WHERE windows_fts MATCH $q
            """ : """
            SELECT w.id, w.snapshot_id, s.ts, s.user, s.machine,
                   w.title, w.app_name, w.process_name, w.process_id, w.is_foreground,
                   SUBSTR(COALESCE(w.text,''), 1, 240) AS snip,
                   length(COALESCE(w.text,'')) AS tlen, w.image_bytes
              FROM windows w
              JOIN snapshots s ON s.id = w.snapshot_id
             WHERE 1=1
            """;

        if (!string.IsNullOrWhiteSpace(appFilter))
        {
            sql += " AND w.app_name = $app";
            cmd.Parameters.AddWithValue("$app", appFilter);
        }
        if (from.HasValue)
        {
            sql += " AND s.ts_unix >= $f";
            cmd.Parameters.AddWithValue("$f", from.Value.ToUnixTimeSeconds());
        }
        if (to.HasValue)
        {
            sql += " AND s.ts_unix <= $t";
            cmd.Parameters.AddWithValue("$t", to.Value.ToUnixTimeSeconds());
        }
        // windows.id is AUTOINCREMENT, so it increases monotonically with capture time.
        // Ordering by w.id DESC therefore yields newest-first WITHOUT sorting on a joined
        // column — the planner just walks the windows primary key backwards and stops at
        // LIMIT. This avoids a full sort of millions of rows on large databases, which is
        // the difference between an instant Browse load and a multi-second hang.
        sql += " ORDER BY w.id DESC LIMIT $lim;";
        cmd.Parameters.AddWithValue("$lim", limit);
        if (hasQuery) cmd.Parameters.AddWithValue("$q", matchQuery);
        cmd.CommandText = sql;

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            hits.Add(new SearchHit(
                WindowId: rdr.GetInt64(0),
                SnapshotId: rdr.GetInt64(1),
                Timestamp: rdr.GetString(2),
                User: rdr.GetString(3),
                Machine: rdr.GetString(4),
                Title: rdr.IsDBNull(5) ? "" : rdr.GetString(5),
                AppName: rdr.IsDBNull(6) ? "" : rdr.GetString(6),
                ProcessName: rdr.IsDBNull(7) ? "" : rdr.GetString(7),
                ProcessId: rdr.GetInt32(8),
                IsForeground: rdr.GetInt32(9) != 0,
                Snippet: rdr.IsDBNull(10) ? "" : rdr.GetString(10),
                TextLength: rdr.GetInt32(11),
                ImageBytes: rdr.GetInt32(12)));
        }
        return hits;
    }

    /// <summary>
    /// Converts arbitrary user input into a safe FTS5 MATCH expression. Each
    /// whitespace-separated token is wrapped in double quotes (embedded quotes are
    /// doubled) so FTS5 special characters — '*', '"', ':', '^', '-', parentheses and
    /// the AND/OR/NOT/NEAR operators — are treated as literal text rather than query
    /// syntax. A leading '*' would otherwise be parsed as a "special query" command and
    /// raise "unknown special query". A trailing '*' is appended to each token so prefix
    /// matching still works. Returns "" when the input has no searchable characters.
    /// </summary>
    private static string BuildFtsMatchQuery(string query)
    {
        var sb = new StringBuilder();
        foreach (var token in query.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            // Skip tokens with no searchable characters so we never emit an empty
            // quoted phrase (e.g. the user typing only "*" or punctuation).
            var hasContent = false;
            foreach (var ch in token)
            {
                if (char.IsLetterOrDigit(ch)) { hasContent = true; break; }
            }
            if (!hasContent) continue;

            var escaped = token.Replace("\"", "\"\"");
            if (sb.Length > 0) sb.Append(' ');
            sb.Append('"').Append(escaped).Append('"').Append('*');
        }
        return sb.ToString();
    }

    public WindowDetail? GetWindowDetail(long windowId, bool includeImage)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        // The image is lazy-loaded: callers pass includeImage:true only when a row is
        // actually selected for preview, so list/search never drags blobs across the wire.
        // COALESCE prefers the new side-table blob and falls back to the legacy inline
        // `image_jpeg` column so pre-split databases still render screenshots.
        cmd.CommandText = $"""
            SELECT w.id, w.snapshot_id, s.ts, s.user, s.machine,
                   w.title, w.app_name, w.process_name, w.process_id, w.executable_path,
                   w.is_foreground, w.bounds_x, w.bounds_y, w.bounds_w, w.bounds_h,
                   w.text, w.ocr_error, w.ocr_duration_ms, w.image_bytes
                   {(includeImage ? ", COALESCE(wi.jpeg, w.image_jpeg)" : "")}
              FROM windows w
              JOIN snapshots s ON s.id = w.snapshot_id
              {(includeImage ? "LEFT JOIN window_images wi ON wi.window_id = w.id" : "")}
             WHERE w.id = $id;
            """;
        cmd.Parameters.AddWithValue("$id", windowId);
        using var rdr = cmd.ExecuteReader();
        if (!rdr.Read()) return null;
        byte[]? img = null;
        if (includeImage && !rdr.IsDBNull(19))
        {
            img = (byte[])rdr.GetValue(19);
        }
        return new WindowDetail(
            WindowId: rdr.GetInt64(0),
            SnapshotId: rdr.GetInt64(1),
            Timestamp: rdr.GetString(2),
            User: rdr.GetString(3),
            Machine: rdr.GetString(4),
            Title: rdr.IsDBNull(5) ? "" : rdr.GetString(5),
            AppName: rdr.IsDBNull(6) ? "" : rdr.GetString(6),
            ProcessName: rdr.IsDBNull(7) ? "" : rdr.GetString(7),
            ProcessId: rdr.GetInt32(8),
            ExecutablePath: rdr.IsDBNull(9) ? "" : rdr.GetString(9),
            IsForeground: rdr.GetInt32(10) != 0,
            BoundsX: rdr.GetInt32(11), BoundsY: rdr.GetInt32(12),
            BoundsW: rdr.GetInt32(13), BoundsH: rdr.GetInt32(14),
            Text: rdr.IsDBNull(15) ? "" : rdr.GetString(15),
            OcrError: rdr.IsDBNull(16) ? null : rdr.GetString(16),
            OcrDurationMs: rdr.GetInt32(17),
            ImageBytes: rdr.GetInt32(18),
            JpegBytes: img);
    }

    public List<SnapshotSummary> ListSnapshots(DateTimeOffset? from, DateTimeOffset? to, int limit = 100)
    {
        var list = new List<SnapshotSummary>();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        var sql = "SELECT id, ts, user, machine, window_count, elapsed_ms FROM snapshots WHERE 1=1";
        if (from.HasValue) { sql += " AND ts_unix >= $f"; cmd.Parameters.AddWithValue("$f", from.Value.ToUnixTimeSeconds()); }
        if (to.HasValue)   { sql += " AND ts_unix <= $t"; cmd.Parameters.AddWithValue("$t", to.Value.ToUnixTimeSeconds()); }
        sql += " ORDER BY ts_unix DESC LIMIT $lim;";
        cmd.Parameters.AddWithValue("$lim", limit);
        cmd.CommandText = sql;
        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            list.Add(new SnapshotSummary(
                rdr.GetInt64(0), rdr.GetString(1), rdr.GetString(2), rdr.GetString(3),
                rdr.GetInt32(4), rdr.GetInt32(5)));
        }
        return list;
    }

    public List<SearchHit> GetWindowsForSnapshot(long snapshotId)
    {
        var hits = new List<SearchHit>();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT w.id, w.snapshot_id, s.ts, s.user, s.machine,
                   w.title, w.app_name, w.process_name, w.process_id, w.is_foreground,
                   SUBSTR(COALESCE(w.text,''), 1, 240) AS snip,
                   length(COALESCE(w.text,'')) AS tlen, w.image_bytes
              FROM windows w
              JOIN snapshots s ON s.id = w.snapshot_id
             WHERE w.snapshot_id = $id
             ORDER BY w.is_foreground DESC, w.id;
            """;
        cmd.Parameters.AddWithValue("$id", snapshotId);
        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            hits.Add(new SearchHit(
                rdr.GetInt64(0), rdr.GetInt64(1), rdr.GetString(2), rdr.GetString(3), rdr.GetString(4),
                rdr.IsDBNull(5) ? "" : rdr.GetString(5), rdr.IsDBNull(6) ? "" : rdr.GetString(6),
                rdr.IsDBNull(7) ? "" : rdr.GetString(7), rdr.GetInt32(8), rdr.GetInt32(9) != 0,
                rdr.IsDBNull(10) ? "" : rdr.GetString(10), rdr.GetInt32(11), rdr.GetInt32(12)));
        }
        return hits;
    }

    public CompactionResult Vacuum()
    {
        var before = GetDatabaseFileBytes();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            PRAGMA wal_checkpoint(TRUNCATE);
            VACUUM;
            PRAGMA wal_checkpoint(TRUNCATE);
            """;
        cmd.ExecuteNonQuery();
        return new CompactionResult(true, before, GetDatabaseFileBytes());
    }

    /// <summary>
    /// Re-encrypts an existing database file with a new key — or removes
    /// encryption (<paramref name="newKey"/> == null), or adds it
    /// (<paramref name="oldKey"/> == null). Uses SQLCipher's
    /// <c>sqlcipher_export</c> to copy every byte through a side file, then
    /// atomically swaps it in. Safe across all transitions: None ↔ UserAccount
    /// ↔ Passphrase, with different passphrases counting as a change.
    /// </summary>
    /// <remarks>
    /// Must NOT be called while a <see cref="Database"/> instance is open on
    /// the same path — callers should dispose the live instance first. We
    /// call <see cref="SqliteConnection.ClearAllPools"/> internally to drop
    /// any cached connection that Microsoft.Data.Sqlite may still be holding
    /// from before disposal, otherwise the file move below fails with a
    /// sharing-violation on Windows. WAL / SHM sidecars are deleted after
    /// the swap so the new file isn't fronted by a stale journal.
    /// </remarks>
    public static void Rekey(string path, string? oldKey, string? newKey)
    {
        if (!File.Exists(path)) return;
        if (string.Equals(oldKey ?? "", newKey ?? "", StringComparison.Ordinal)) return;

        SqliteConnection.ClearAllPools();

        var tempPath = path + ".rekey-tmp";
        TryDelete(tempPath);
        TryDelete(tempPath + "-wal");
        TryDelete(tempPath + "-shm");

        // SQLCipher quirk: `ATTACH DATABASE 'foo.db' AS x KEY '...'` on a
        // non-existent destination path fails with SQLITE_CANTOPEN (error 14)
        // any time the source connection's cipher state doesn't match what's
        // being requested for the destination — including the very common
        // plaintext<->encrypted case. The documented workaround is to
        // materialize the destination file first using a separate connection
        // configured exactly the way we want the destination to be, then
        // ATTACH it. An empty file with the right cipher state is everything
        // SQLCipher needs; `sqlcipher_export` then populates it.
        var seedCsb = new SqliteConnectionStringBuilder
        {
            DataSource = tempPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        };
        if (!string.IsNullOrEmpty(newKey)) seedCsb.Password = newKey;
        using (var seed = new SqliteConnection(seedCsb.ConnectionString))
        {
            seed.Open();
            // For encrypted destinations, force SQLCipher to write its header
            // pages by running a trivial PRAGMA. Without this, an unused
            // connection sometimes leaves a zero-byte file, which then fails
            // when re-opened via ATTACH.
            if (!string.IsNullOrEmpty(newKey))
            {
                using var pragma = seed.CreateCommand();
                pragma.CommandText = "PRAGMA user_version;";
                try { pragma.ExecuteScalar(); } catch { /* best-effort */ }
            }
        }
        SqliteConnection.ClearAllPools();

        var csb = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWrite,
            Pooling = false,
        };
        if (!string.IsNullOrEmpty(oldKey)) csb.Password = oldKey;

        using (var src = new SqliteConnection(csb.ConnectionString))
        {
            src.Open();
            // Flush WAL into the main file so sqlcipher_export sees everything.
            using (var checkpoint = src.CreateCommand())
            {
                checkpoint.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                try { checkpoint.ExecuteNonQuery(); } catch { /* best-effort */ }
            }

            using var cmd = src.CreateCommand();
            var attachKey = QuoteSqlLiteral(newKey ?? "");
            var attachPath = QuoteSqlLiteral(tempPath);

            // Issue the three statements separately. Microsoft.Data.Sqlite can
            // execute multi-statement CommandText, but splitting them gives us
            // a per-step stack frame if anything throws — much easier to triage.
            cmd.CommandText = $"ATTACH DATABASE {attachPath} AS rekey KEY {attachKey};";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "SELECT sqlcipher_export('rekey');";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "DETACH DATABASE rekey;";
            cmd.ExecuteNonQuery();
        }

        SqliteConnection.ClearAllPools();

        var backup = path + ".rekey-bak";
        TryDelete(backup);
        File.Move(path, backup);
        try
        {
            File.Move(tempPath, path);
            TryDelete(path + "-wal");
            TryDelete(path + "-shm");
            TryDelete(backup);
        }
        catch
        {
            // Roll back: restore the original file.
            TryDelete(path);
            File.Move(backup, path);
            throw;
        }
    }

    private static string QuoteSqlLiteral(string s) => "'" + s.Replace("'", "''") + "'";
    private static void TryDelete(string p) { try { if (File.Exists(p)) File.Delete(p); } catch { } }

    public long GetDatabaseFileBytes()
    {
        long total = 0;
        AddIfExists(DatabasePath);
        AddIfExists(DatabasePath + "-wal");
        AddIfExists(DatabasePath + "-shm");
        return total;

        void AddIfExists(string path)
        {
            if (File.Exists(path))
                total += new FileInfo(path).Length;
        }
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
    }
}

public sealed record DatabaseStats(
    long Snapshots,
    long Windows,
    long ImageBytes,
    string? FirstTimestamp,
    string? LastTimestamp);

public sealed record SearchHit(
    long WindowId, long SnapshotId, string Timestamp,
    string User, string Machine,
    string Title, string AppName, string ProcessName, int ProcessId,
    bool IsForeground, string Snippet, int TextLength, int ImageBytes);

public sealed record SnapshotSummary(
    long Id, string Timestamp, string User, string Machine, int WindowCount, int ElapsedMs);

public sealed record WindowDetail(
    long WindowId, long SnapshotId, string Timestamp, string User, string Machine,
    string Title, string AppName, string ProcessName, int ProcessId, string ExecutablePath,
    bool IsForeground, int BoundsX, int BoundsY, int BoundsW, int BoundsH,
    string Text, string? OcrError, int OcrDurationMs, int ImageBytes, byte[]? JpegBytes);

public sealed record CapturedWindow(WindowRecord Record, byte[]? Jpeg);
