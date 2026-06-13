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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalRecall;

public partial class MainForm : Form
{
    private AppSettings? settings;
    private Database? db;
    private OcrService? ocr;
    private SnapshotService? service;

    private System.Threading.Timer? timer;
    private CancellationTokenSource? cts;
    private int captureBusy;
    private DateTime lastRetentionUtc = DateTime.MinValue;

    // Tray + lifecycle
    private NotifyIcon? trayIcon;
    private ToolStripMenuItem? trayStartItem;
    private ToolStripMenuItem? trayStopItem;
    private bool reallyExit;
    private bool shouldStartHidden;
    private bool autoStartedCapture;
    // CLI override of the auto-start-capture decision. null = honour settings.WasCapturing.
    private bool? captureOverride;

    public MainForm() : this(new LaunchOptions()) { }

    /// <summary>Legacy convenience ctor — keeps older callers compiling.</summary>
    public MainForm(bool startMinimized) : this(new LaunchOptions { StartMinimized = startMinimized }) { }

    public MainForm(LaunchOptions opts)
    {
        InitializeComponent();
        // Don't run any runtime wiring at design time — the Visual Studio
        // designer instantiates the form with the parameterless ctor.
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
        InitializeApp(opts);
    }

    private void InitializeApp(LaunchOptions opts)
    {
        settings = AppSettings.Load();
        captureOverride = opts.CaptureOverride;

        var mode = "Unencrypted";
        if (settings.EncryptionMode == EncryptionMode.Passphrase) mode = "Password Encrypted";
        if (settings.EncryptionMode == EncryptionMode.UserAccount) mode = "User Account Encrypted";


        this.Text = "TotalRecall (" + mode + ")";

        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        // --tray forces hidden-in-tray regardless of MinimizeToTray setting.
        // --minimized + MinimizeToTray setting also hides to tray. Otherwise --minimized
        // just minimizes the window normally.
        if (opts.StartInTray || (opts.StartMinimized && settings.MinimizeToTray))
        {
            shouldStartHidden = true;
            ShowInTaskbar = false;
        }
        else if (opts.StartMinimized)
        {
            WindowState = FormWindowState.Minimized;
        }

        capturePanel.BindSettings(settings);
        settingsPanel.BindSettings(settings);

        tabs.SelectedIndexChanged += (_, _) =>
        {
            if (tabs.SelectedTab == tabBrowse) browsePanel.RefreshIfNeeded();
        };

        capturePanel.StartClicked += async (_, _) =>
        {
            await StartAsync();
            PersistCaptureIntent(true);
        };
        capturePanel.StopClicked  += (_, _) =>
        {
            Stop();
            PersistCaptureIntent(false);
        };
        capturePanel.CaptureNowClicked += async (_, _) => await CaptureNowAsync();
        capturePanel.OpenDbFolderClicked += (_, _) => OpenDbFolder();

        settingsPanel.SettingsSaved += (_, _) =>
        {
            DisposeServices();
            capturePanel.RefreshFromSettings();
            UpdateDbLabel();
            TryAttachExistingDatabase();
            // Force the retention sweep to re-run on next tick / now, with the new thresholds.
            lastRetentionUtc = DateTime.MinValue;
            RunRetentionSweep(force: true);
            // If the user toggled "Minimize to tray" off while we're hidden, restore the window.
            if (settings != null && !settings.MinimizeToTray && !Visible)
                ShowFromTray();
        };

        settingsPanel.PurgeRequested += (_, _) => RunRetentionSweep(force: true, compactNow: true);
        settingsPanel.ClearDatabaseRequested += (_, _) => ClearDatabaseNow();

        FormClosing += OnFormClosingHandler;

        quitBtn.Click += (_, _) => { reallyExit = true; Close(); };

        BuildTrayIcon();

        UpdateDbLabel();
        TryAttachExistingDatabase();
        RunRetentionSweep(force: true);

        // Resume capture if it was running when the app was last closed.
        // CLI override (--capture-on / --capture-off) wins over remembered state.
        // The minimized-tray autostart path runs its own start in OnInitialHiddenStartup,
        // so we skip here to avoid a double-start.
        bool wantCapture = captureOverride ?? settings.WasCapturing;
        if (wantCapture && !shouldStartHidden && !autoStartedCapture)
        {
            autoStartedCapture = true;
            _ = StartAsync();
        }
    }

    private void PersistCaptureIntent(bool running)
    {
        if (settings == null) return;
        if (settings.WasCapturing == running) return;
        settings.WasCapturing = running;
        try { settings.Save(); } catch { /* swallow — best-effort persistence */ }
    }

    // --- Tray + window-state plumbing ----------------------------------

    private void BuildTrayIcon()
    {
        Icon? icon = null;
        try { icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
        icon ??= SystemIcons.Application;

        var menu = new ContextMenuStrip
        {
            BackColor = Color.FromArgb(235, 235, 238),
            ForeColor = Color.FromArgb(28, 28, 30),
        };
        menu.Items.Add("Open TotalRecall", null, (_, _) => ShowFromTray());
        menu.Items.Add(new ToolStripSeparator());
        trayStartItem = new ToolStripMenuItem("Start capture", null, async (_, _) => await StartAsync());
        trayStopItem  = new ToolStripMenuItem("Stop capture", null, (_, _) => Stop()) { Enabled = false };
        var trayCaptureNow = new ToolStripMenuItem("Capture now", null, async (_, _) => await CaptureNowAsync());
        menu.Items.Add(trayStartItem);
        menu.Items.Add(trayStopItem);
        menu.Items.Add(trayCaptureNow);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => { reallyExit = true; Close(); });

        trayIcon = new NotifyIcon
        {
            Icon = icon,
            Text = "TotalRecall",
            Visible = false,
            ContextMenuStrip = menu,
        };
        trayIcon.DoubleClick += (_, _) => ShowFromTray();
    }

    /// <summary>
    /// SetVisibleCore override lets us start with the window completely hidden
    /// (no flash on screen) when launched at login with tray mode on.
    /// </summary>
    protected override void SetVisibleCore(bool value)
    {
        if (shouldStartHidden && value)
        {
            if (!IsHandleCreated) CreateHandle();
            shouldStartHidden = false;
            base.SetVisibleCore(false);
            BeginInvoke(new Action(OnInitialHiddenStartup));
            return;
        }
        base.SetVisibleCore(value);
    }

    private async void OnInitialHiddenStartup()
    {
        if (trayIcon != null) trayIcon.Visible = true;

        // CLI override (--capture-on / --capture-off) wins over remembered state.
        bool wantCapture = captureOverride ?? (settings?.WasCapturing ?? true);

        try
        {
            trayIcon?.ShowBalloonTip(2500, "TotalRecall",
                wantCapture
                    ? "Running in the background — capture starting."
                    : "Running in the background — capture is OFF.",
                ToolTipIcon.Info);
        }
        catch { }

        if (wantCapture && !autoStartedCapture)
        {
            autoStartedCapture = true;
            try { await StartAsync(); } catch { /* surfaced via log */ }
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized && settings is { MinimizeToTray: true })
            HideToTray();
    }

    private void OnFormClosingHandler(object? sender, FormClosingEventArgs e)
    {
        // Intercept the X button when the user wants minimize-to-tray.
        if (!reallyExit && settings is { MinimizeToTray: true } && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }
        Stop();
        DisposeServices();
        try { settings?.Save(); } catch { }
        if (trayIcon != null)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
            trayIcon = null;
        }
    }

    private void HideToTray()
    {
        if (trayIcon != null) trayIcon.Visible = true;
        ShowInTaskbar = false;
        Hide();
    }

    private void ShowFromTray()
    {
        Show();
        if (WindowState == FormWindowState.Minimized)
            WindowState = FormWindowState.Normal;
        ShowInTaskbar = true;
        Activate();
        BringToFront();
        Focus();
    }

    private void UpdateTrayMenuState(bool running)
    {
        if (trayStartItem != null) trayStartItem.Enabled = !running;
        if (trayStopItem  != null) trayStopItem.Enabled  = running;
    }

    // --- Capture lifecycle ---------------------------------------------

    private async Task StartAsync()
    {
        if (settings == null) return;
        try { EnsureServices(); }
        catch (Exception ex)
        {
            ShowError("Startup error", ex.Message);
            return;
        }
        cts = new CancellationTokenSource();
        var intervalMs = settings.IntervalSeconds * 1000;
        capturePanel.SetRunningState(true);
        UpdateTrayMenuState(true);
        SetStatus($"Running. Interval = {settings.IntervalSeconds}s");
        capturePanel.Log($"Started. Interval = {settings.IntervalSeconds}s, JPEG q={settings.JpegQuality}, encryption={settings.EncryptionMode}");
        await TickAsync();
        timer = new System.Threading.Timer(async _ => await TickAsync(), null, intervalMs, intervalMs);
    }

    private void Stop()
    {
        try { timer?.Dispose(); timer = null; cts?.Cancel(); } catch { }
        capturePanel.SetRunningState(false);
        UpdateTrayMenuState(false);
        SetStatus("Stopped.");
        capturePanel.Log("Stopped.");
    }

    private async Task CaptureNowAsync()
    {
        try
        {
            EnsureServices();
            await TickAsync();
        }
        catch (Exception ex) { ShowError("Capture error", ex.Message); }
    }

    private async Task TickAsync()
    {
        if (Interlocked.Exchange(ref captureBusy, 1) == 1)
        {
            capturePanel.Log("Skipping tick — previous capture still running.");
            return;
        }
        try
        {
            if (service == null) return;
            var ct = cts?.Token ?? CancellationToken.None;
            var result = await service.CaptureOnceAsync(ct);
            var snapshotLabel = result.SnapshotId > 0 ? $"snapshot #{result.SnapshotId}" : "no-change tick";
            capturePanel.Log($"[{DateTime.Now:HH:mm:ss}] {snapshotLabel}: {result.StoredWindowCount}/{result.WindowCount} windows stored, {result.SkippedUnchanged} unchanged skipped, {result.ImageBytes / 1024} KB JPEG, {result.ElapsedMs} ms");
            UpdateDbLabel();
            browsePanel.InvalidateData();
            RunRetentionSweep(force: false);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { capturePanel.Log("Tick error: " + ex.Message); }
        finally { Interlocked.Exchange(ref captureBusy, 0); }
    }

    /// <summary>
    /// Applies the user's retention policy. Off-thread, best-effort, throttled to once
    /// every 30 minutes unless <paramref name="force"/> is true (Save / Purge now / startup).
    /// </summary>
    private void RunRetentionSweep(bool force, bool compactNow = false)
    {
        if (settings == null || db == null) return;
        if (!settings.PurgeImagesEnabled && !settings.PurgeAllEnabled) return;
        if (!force && (DateTime.UtcNow - lastRetentionUtc).TotalMinutes < 30) return;

        var snapshotSettings = settings;
        var snapshotDb = db;
        lastRetentionUtc = DateTime.UtcNow;

        Task.Run(() =>
        {
            try
            {
                var r = snapshotDb.ApplyRetention(snapshotSettings);
                CompactionResult compaction = default;
                var didCompact = false;
                if (r.Changed && ShouldCompactAfterRetention(snapshotSettings, compactNow))
                {
                    compaction = snapshotDb.Vacuum();
                    didCompact = compaction.Ran;
                    snapshotSettings.LastCompactionUtc = DateTime.UtcNow.ToString("o");
                    try { snapshotSettings.Save(); } catch (Exception ex) { BeginInvoke(new Action(() => capturePanel.Log("[retention] compaction timestamp save error: " + ex.Message))); }
                }

                if (r.Changed || didCompact)
                {
                    BeginInvoke(new Action(() =>
                    {
                        var compactMsg = didCompact
                            ? $", compacted DB ({FormatBytes(compaction.ReclaimedBytes)} reclaimed)"
                            : "";
                        capturePanel.Log($"[retention] purged {r.RowsDeleted} row(s), stripped {r.ImagesPurged} image(s){compactMsg}.");
                        UpdateDbLabel();
                        browsePanel.InvalidateData();
                    }));
                }
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() => capturePanel.Log("[retention] error: " + ex.Message)));
            }
        });
    }

    /// <summary>
    /// Wipes the entire database — every snapshot, image, OCR row, and FTS5 index entry.
    /// Runs off-thread so the UI stays responsive on large DBs. Refreshes the Browse panel
    /// when finished. Triggered by the Settings → "Clear database" button.
    /// </summary>
    private void ClearDatabaseNow()
    {
        if (db == null)
        {
            capturePanel.Log("[clear] no database attached.");
            return;
        }

        var snapshotDb = db;
        var snapshotSettings = settings;

        Task.Run(() =>
        {
            try
            {
                var deleted = snapshotDb.ClearAll();
                var compacted = snapshotDb.Vacuum();
                if (snapshotSettings != null)
                {
                    snapshotSettings.LastCompactionUtc = DateTime.UtcNow.ToString("o");
                    try { snapshotSettings.Save(); }
                    catch (Exception ex) { BeginInvoke(new Action(() => capturePanel.Log("[clear] compaction timestamp save error: " + ex.Message))); }
                }
                BeginInvoke(new Action(() =>
                {
                    capturePanel.Log($"[clear] database wiped: {deleted} snapshot(s) deleted, compacted DB ({FormatBytes(compacted.ReclaimedBytes)} reclaimed).");
                    UpdateDbLabel();
                    browsePanel.InvalidateData();
                    browsePanel.RefreshIfNeeded();
                }));
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() => ShowError("Clear database failed", ex.Message)));
            }
        });
    }

    private static bool ShouldCompactAfterRetention(AppSettings settings, bool compactNow)
    {
        if (!settings.CompactAfterRetentionEnabled && !compactNow) return false;
        if (compactNow) return true;
        if (settings.CompactAfterRetentionHours <= 0) return true;
        if (!DateTimeOffset.TryParse(settings.LastCompactionUtc, out var lastCompaction)) return true;
        return DateTimeOffset.UtcNow - lastCompaction.ToUniversalTime() >= TimeSpan.FromHours(settings.CompactAfterRetentionHours);
    }

    private void EnsureServices()
    {
        if (settings == null) throw new InvalidOperationException("Settings not loaded.");
        if (service != null) return;

        if (db == null)
        {
            string? key;
            try { key = KeyVault.GetKey(settings); }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot resolve database key: " + ex.Message, ex);
            }

            db = new Database(settings.DatabasePath, key);
            try { db.VerifyAccessible(); }
            catch (Exception ex)
            {
                db.Dispose(); db = null;
                throw new InvalidOperationException(
                    "Could not open the encrypted database with the configured key. " +
                    "If you changed the encryption mode or passphrase, point at a fresh DB path or restore your key. " +
                    "Details: " + ex.Message, ex);
            }

            browsePanel.AttachDatabase(db);
        }

        var tessdata = Path.Combine(AppContext.BaseDirectory, "tessdata");
        ocr = new OcrService(tessdata, settings.OcrLanguage, settings.OcrMaxDimension);

        service = new SnapshotService(ocr, db, settingsProvider: () => settings);
    }

    /// <summary>
    /// Open the configured DB read-only-style so the Browse tab works before capture starts.
    /// Best-effort: errors (missing file, wrong key) are surfaced as a status hint, not thrown.
    /// </summary>
    private void TryAttachExistingDatabase()
    {
        if (settings == null) return;
        if (db != null) { browsePanel.AttachDatabase(db); browsePanel.InvalidateData(); return; }
        if (!File.Exists(settings.DatabasePath)) return;

        try
        {
            var key = KeyVault.GetKey(settings);
            var candidate = new Database(settings.DatabasePath, key);
            candidate.VerifyAccessible();
            db = candidate;
            browsePanel.AttachDatabase(db);
            browsePanel.InvalidateData();
        }
        catch (Exception ex)
        {
            statusLbl.Text = "DB not opened: " + ex.Message;
        }
    }

    private void DisposeServices()
    {
        try { service = null; ocr?.Dispose(); ocr = null; db?.Dispose(); db = null; } catch { }
        browsePanel.AttachDatabase(null);
    }

    private void OpenDbFolder()
    {
        if (settings == null) return;
        try
        {
            var dir = Path.GetDirectoryName(settings.DatabasePath) ?? AppSettings.AppDataDir;
            Directory.CreateDirectory(dir);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = dir, UseShellExecute = true,
            });
        }
        catch (Exception ex) { ShowError("Open folder failed", ex.Message); }
    }

    private void UpdateDbLabel()
    {
        if (settings == null) return;
        try
        {
            string txt;
            if (File.Exists(settings.DatabasePath))
            {
                var len = new FileInfo(settings.DatabasePath).Length;
                txt = $"DB: {settings.DatabasePath}  ·  {FormatBytes(len)}  ·  enc={settings.EncryptionMode}";
            }
            else
            {
                txt = $"DB: {settings.DatabasePath}  (not created yet)";
            }
            if (InvokeRequired) BeginInvoke(() => dbLbl.Text = txt);
            else dbLbl.Text = txt;
        }
        catch { }
    }

    private void SetStatus(string s)
    {
        if (InvokeRequired) BeginInvoke(() => statusLbl.Text = s);
        else statusLbl.Text = s;
    }

    private void ShowError(string title, string msg)
    {
        capturePanel.Log("ERROR: " + msg);
        MessageBox.Show(this, msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static string FormatBytes(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double v = bytes; int u = 0;
        while (v >= 1024 && u < units.Length - 1) { v /= 1024; u++; }
        return $"{v:0.##} {units[u]}";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            timer?.Dispose();
            cts?.Dispose();
            DisposeServices();
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
                trayIcon = null;
            }
        }
        base.Dispose(disposing);
    }
}
