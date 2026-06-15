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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// The application's main window. Owns the capture pipeline lifecycle, the system tray,
/// the hamburger menu, the embedded browse pane, and the singleton ActivityLog +
/// Settings sub-windows.
/// </summary>
/// <remarks>
/// <para><b>Layout:</b></para>
/// <list type="bullet">
///   <item><b>Header</b> — app title, inline capture status pill ("Recording" / "Idle"),
///     interval / JPEG / encryption summary, last-snapshot stamp, Start/Stop buttons,
///     hamburger menu button, Quit button. All the <c>cap*</c> named fields below were
///     transplanted in from the old <c>CaptureBar</c> control.</item>
///   <item><b>Body</b> — <see cref="BrowsePanel"/> docked fill.</item>
///   <item><b>Footer</b> — DB path + size status strip.</item>
/// </list>
/// <para><b>Capture lifecycle:</b> a single <see cref="System.Threading.Timer"/> drives
/// <see cref="TickAsync"/> on the threadpool. <see cref="captureBusy"/> is an
/// <see cref="Interlocked"/> guard so a slow tick (heavy OCR) never re-enters.</para>
/// <para><b>Tray vs. window:</b> closing the window minimises to tray unless the user
/// chose "Exit" from the tray menu (sets <see cref="reallyExit"/>). This is intentional —
/// users on the auto-start shortcut expect the app to keep recording in the background.</para>
/// <para><b>Shortcuts:</b> <see cref="ProcessCmdKey"/> is the sole router for all
/// global keyboard shortcuts (Ctrl+L, Ctrl+,, Ctrl+Shift+D, F5, Ctrl+F, Ctrl+ +/-, …).
/// The hamburger menu uses <c>ShortcutKeyDisplayString</c> only — never
/// <c>ShortcutKeys</c> — to display the binding without competing with the router.</para>
/// </remarks>
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
    // Tracked so a graceful shutdown can wait for any background retention
    // pass to complete before we dispose the DB out from under it.
    private Task? lastRetentionTask;
    // True once OnFormClosingHandler decides we're really shutting down.
    // Marshalled callbacks (BeginInvoke from TickAsync / retention) check
    // this to bail out before touching disposed controls.
    private volatile bool shuttingDown;

    // Tray + lifecycle
    private NotifyIcon? trayIcon;
    private ToolStripMenuItem? trayStartItem;
    private ToolStripMenuItem? trayStopItem;
    private bool reallyExit;
    private bool shouldStartHidden;
    private bool autoStartedCapture;
    // CLI override of the auto-start-capture decision. null = honour settings.WasCapturing.
    private bool? captureOverride;

    // Auxiliary windows (singletons; reopened windows just bring the existing one to front).
    private ActivityLogForm? activityLogForm;
    private SettingsForm? settingsForm;
    private ContextMenuStrip? hamburgerMenu;

    public MainForm() : this(new LaunchOptions()) { }

    /// <summary>Legacy convenience ctor — keeps older callers compiling.</summary>
    public MainForm(bool startMinimized) : this(new LaunchOptions { StartMinimized = startMinimized }) { }

    public MainForm(LaunchOptions opts)
    {
        InitializeComponent();
        // Don't run any runtime wiring at design time — the Visual Studio
        // designer instantiates the form with the parameterless ctor.
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
        // Form-level double-buffering: smooths the routine repaints on the
        // header bar (capture-status pill, "Last:" timestamp) without the
        // drag-stutter side-effect of WS_EX_COMPOSITED, which forces the
        // entire window to render through an extra back-buffer per paint.
        DoubleBuffered = true;
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

        RefreshCaptureSummary();

        capStartBtn.Click += async (_, _) =>
        {
            await StartAsync();
            PersistCaptureIntent(true);
        };
        capStopBtn.Click += (_, _) =>
        {
            Stop();
            PersistCaptureIntent(false);
        };

        BuildHamburgerMenu();
        menuBtn.Click += (_, _) =>
        {
            if (hamburgerMenu == null) return;
            hamburgerMenu.Show(menuBtn, new Point(0, menuBtn.Height));
        };

        FormClosing += OnFormClosingHandler;
        // Restoring the window from tray / Alt+Tab fires Activated — use it to
        // immediately surface any data the capture loop produced while the
        // window was hidden, rather than waiting for the next tick.
        Activated += (_, _) => browsePanel.RefreshIfNeeded();

        quitBtn.Click += (_, _) => { reallyExit = true; Close(); };

        BuildTrayIcon();

        UpdateDbLabel();
        TryAttachExistingDatabase();
        RunRetentionSweep(force: true);
        // Force an initial Browse populate so the user sees data immediately.
        browsePanel.RefreshIfNeeded();

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

    // --- Hamburger menu --------------------------------------------------

    private void BuildHamburgerMenu()
    {
        hamburgerMenu = new ContextMenuStrip
        {
            BackColor = Color.FromArgb(245, 245, 247),
            ForeColor = Theme.Fg,
            Font = Theme.UiFont,
            ShowImageMargin = false,
        };
        hamburgerMenu.Items.Add(MakeMenuItem("Refresh results",  "F5",           () => browsePanel.ForceRefresh()));
        hamburgerMenu.Items.Add(new ToolStripSeparator());
        hamburgerMenu.Items.Add(MakeMenuItem("Activity log",     "Ctrl+L",       OpenActivityLog));
        hamburgerMenu.Items.Add(MakeMenuItem("Settings…",        "Ctrl+,",       OpenSettings));
        hamburgerMenu.Items.Add(MakeMenuItem("Open DB folder",   "Ctrl+Shift+D", OpenDbFolder));
        hamburgerMenu.Items.Add(new ToolStripSeparator());
        hamburgerMenu.Items.Add(MakeMenuItem("About TotalRecall…", null,         ShowAbout));
    }

    private static ToolStripMenuItem MakeMenuItem(string text, string? shortcut, Action onClick)
    {
        var item = new ToolStripMenuItem(text, null, (_, _) => onClick());
        if (!string.IsNullOrEmpty(shortcut)) item.ShortcutKeyDisplayString = shortcut;
        return item;
    }

    private void OpenActivityLog()
    {
        if (activityLogForm == null || activityLogForm.IsDisposed)
        {
            activityLogForm = new ActivityLogForm();
            activityLogForm.FormClosed += (_, _) => activityLogForm = null;
            CenterChildOnThis(activityLogForm);
            activityLogForm.Show(this);
        }
        else
        {
            if (activityLogForm.WindowState == FormWindowState.Minimized)
                activityLogForm.WindowState = FormWindowState.Normal;
            activityLogForm.BringToFront();
            activityLogForm.Activate();
        }
    }

    private void OpenSettings()
    {
        if (settings == null) return;
        if (settingsForm == null || settingsForm.IsDisposed)
        {
            // Snapshot the encryption identity BEFORE the user changes anything.
            // SettingsPanel mutates the same AppSettings instance in place when
            // the user clicks Save, so we have to capture the "old" values now.
            var oldMode = settings.EncryptionMode;
            var oldPass = settings.RuntimePassphrase;
            string? oldKey;
            try { oldKey = KeyVault.GetKey(settings); }
            catch { oldKey = null; /* shouldn't happen — current DB already opens */ }

            settingsForm = new SettingsForm(settings);
            settingsForm.SettingsSaved += async (_, _) =>
            {
                // If capture is currently running, we need to fully stop the
                // timer (not just null out services) before the rekey can run,
                // because background ticks would otherwise see a null service
                // and silently no-op for the rest of the session even after
                // the DB is back. We remember the running state so we can
                // restart capture once re-encryption completes.
                bool wasCapturing = timer != null;
                if (wasCapturing) Stop(waitForInflight: true);
                DisposeServices();
                RefreshCaptureSummary();

                // Re-encrypt the existing DB file in place if the encryption mode
                // or passphrase changed. Without this, the next AttachExistingDatabase
                // call would fail with "file is not a database" (wrong key) or with
                // a SQLCipher decrypt error.
                //
                // For non-trivial DBs the rekey is slow enough (seconds-to-minutes
                // on 100+ MB files) that we must NOT do it on the UI thread.
                // ReencryptionDialog runs the rekey on a background task while
                // showing a progress bar driven by the temp file's growth, and
                // blocks the rest of the app (modal) until it's done. The user
                // can request a quit during the rekey; if they do, the dialog
                // keeps running, finishes safely, and we Application.Exit on
                // return so the file move + journal cleanup never gets cut off.
                var newMode = settings.EncryptionMode;
                var newPass = settings.RuntimePassphrase;
                var changed = newMode != oldMode
                            || !string.Equals(oldPass ?? "", newPass ?? "", StringComparison.Ordinal);

                if (changed && File.Exists(settings.DatabasePath))
                {
                    string? newKey;
                    try { newKey = KeyVault.GetKey(settings); }
                    catch (Exception ex)
                    {
                        ShowError("Encryption change failed",
                            "Could not resolve the new encryption key. The database was left unchanged.\r\n\r\n" + ex.Message);
                        TryAttachExistingDatabase();
                        if (wasCapturing) { try { await StartAsync(); } catch (Exception ex2) { ShowError("Restart failed", ex2.Message); } }
                        return;
                    }

                    LogSink.Log($"[encryption] re-encrypting DB: {oldMode} → {newMode}");
                    using (var dlg = new ReencryptionDialog(settings.DatabasePath, oldKey, newKey,
                        DescribeEncryption(oldMode), DescribeEncryption(newMode)))
                    {
                        CenterChildOnThis(dlg);
                        dlg.ShowDialog(this);

                        if (dlg.Error != null)
                        {
                            LogSink.Log("[encryption] re-encryption failed: " + dlg.Error.Message);
                            ShowError("Re-encryption failed",
                                "Could not re-encrypt the database with the new key. " +
                                "Your original database file has been preserved.\r\n\r\n" + dlg.Error.Message);
                            // Don't quit even if user clicked Quit: the DB is unchanged,
                            // so they probably want another shot.
                            TryAttachExistingDatabase();
                            if (wasCapturing) { try { await StartAsync(); } catch (Exception ex2) { ShowError("Restart failed", ex2.Message); } }
                            return;
                        }

                        LogSink.Log("[encryption] re-encryption complete.");

                        if (dlg.QuitRequested)
                        {
                            // User asked to quit while the rekey was running.
                            // Rekey is now finished and the file is safe — exit.
                            // OnFormClosingHandler still runs cleanly because
                            // services were already disposed before the dialog.
                            shuttingDown = true;
                            Application.Exit();
                            return;
                        }
                    }
                }

                UpdateDbLabel();
                TryAttachExistingDatabase();
                lastRetentionUtc = DateTime.MinValue;
                RunRetentionSweep(force: true);

                // Restart the capture timer if it was running before the
                // settings change. EnsureServices() inside StartAsync()
                // rebuilds ocr + service for the now-re-attached DB.
                if (wasCapturing)
                {
                    try { await StartAsync(); }
                    catch (Exception ex) { ShowError("Restart failed", ex.Message); }
                }

                // Force-refresh the results list now so the user sees data
                // immediately after re-encryption — without this, the panel
                // sits empty until the next capture tick (or never, if
                // capture wasn't running) even though the DB is fully open.
                browsePanel.InvalidateData();
                browsePanel.ForceRefresh();

                if (settings != null && !settings.MinimizeToTray && !Visible) ShowFromTray();
            };
            settingsForm.PurgeRequested += (_, _) => RunRetentionSweep(force: true, compactNow: true);
            settingsForm.ClearDatabaseRequested += (_, _) => ClearDatabaseNow();
            settingsForm.FormClosed += (_, _) => settingsForm = null;
            CenterChildOnThis(settingsForm);
            settingsForm.Show(this);
        }
        else
        {
            if (settingsForm.WindowState == FormWindowState.Minimized)
                settingsForm.WindowState = FormWindowState.Normal;
            settingsForm.BringToFront();
            settingsForm.Activate();
        }
    }

    /// <summary>
    /// Manually centers a non-modal child form on top of <c>this</c>. WinForms'
    /// <see cref="FormStartPosition.CenterParent"/> only works with
    /// <see cref="Form.ShowDialog()"/>; for <see cref="Form.Show(IWin32Window)"/>
    /// the runtime ignores it and falls back to <see cref="FormStartPosition.WindowsDefaultLocation"/>.
    /// Clamps to the parent's screen so the child can't end up off-screen on
    /// multi-monitor setups where the parent is near a screen edge.
    /// </summary>
    private void CenterChildOnThis(Form child)
    {
        try
        {
            child.StartPosition = FormStartPosition.Manual;
            var ownerBounds = WindowState == FormWindowState.Minimized
                ? Screen.FromControl(this).WorkingArea
                : Bounds;
            var x = ownerBounds.Left + Math.Max(0, (ownerBounds.Width  - child.Width)  / 2);
            var y = ownerBounds.Top  + Math.Max(0, (ownerBounds.Height - child.Height) / 2);
            var screen = Screen.FromPoint(new Point(x + child.Width / 2, y + child.Height / 2)).WorkingArea;
            x = Math.Max(screen.Left, Math.Min(x, screen.Right  - child.Width));
            y = Math.Max(screen.Top,  Math.Min(y, screen.Bottom - child.Height));
            child.Location = new Point(x, y);
        }
        catch { /* positioning is cosmetic — let WinForms place it if anything goes sideways */ }
    }

    private void ShowAbout()
    {
        var asm = typeof(MainForm).Assembly;
        var ver = asm.GetName().Version?.ToString(3) ?? "?";
        var msg =
            $"TotalRecall v{ver}\r\n" +
            "Local screen-activity indexer.\r\n\r\n" +
            "https://github.com/ilyafainberg/TotalRecall\r\n\r\n" +
            "Licensed under GPL-3.0-or-later.";
        MessageBox.Show(this, msg, "About TotalRecall", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // --- Keyboard shortcuts ---------------------------------------------

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Hamburger shortcuts first.
        if (keyData == (Keys.Control | Keys.L)) { OpenActivityLog(); return true; }
        if (keyData == (Keys.Control | Keys.Oemcomma)) { OpenSettings(); return true; }
        if (keyData == (Keys.Control | Keys.Shift | Keys.D)) { OpenDbFolder(); return true; }

        // Forward Browse-panel shortcuts (zoom, F5, Ctrl+F).
        if (browsePanel.TryHandleShortcut(keyData)) return true;

        return base.ProcessCmdKey(ref msg, keyData);
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
        menu.Items.Add(trayStartItem);
        menu.Items.Add(trayStopItem);
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
        shuttingDown = true;
        // Block briefly so the in-flight capture tick + retention sweep can
        // unwind cleanly. Without this, DisposeServices() races against
        // background threads still holding the DB / OCR engine — crash on close.
        Stop(waitForInflight: true);
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

    private void RefreshCaptureSummary()
    {
        if (settings == null) return;
        var mode = settings.CaptureForegroundOnly ? "foreground" : "all windows";
        var change = settings.EnableChangeDetection ? "  ·  skip unchanged" : "";
        var store = settings.StoreScreenshots ? "stored" : "OCR only";
        capInfoLbl.Text =
            $"Every {settings.IntervalSeconds}s  ·  JPEG q{settings.JpegQuality} ({store})  ·  {mode}{change}  ·  enc={settings.EncryptionMode}";
    }

    private void SetCaptureRunningState(bool running)
    {
        capDotLbl.ForeColor = running ? Theme.Ok : Color.FromArgb(170, 170, 178);
        capStateLbl.Text = running ? "Recording" : "Idle";
        capStateLbl.ForeColor = running ? Theme.Ok : Theme.FgMuted;
        capStartBtn.Enabled = !running;
        capStopBtn.Enabled  = running;
    }

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
        SetCaptureRunningState(true);
        UpdateTrayMenuState(true);
        SetStatus($"Running. Interval = {settings.IntervalSeconds}s");
        LogSink.Log($"Started. Interval = {settings.IntervalSeconds}s, JPEG q={settings.JpegQuality}, encryption={settings.EncryptionMode}");
        await TickAsync();
        timer = new System.Threading.Timer(async _ => await TickAsync(), null, intervalMs, intervalMs);
    }

    /// <summary>
    /// Stops the capture loop. When <paramref name="waitForInflight"/> is
    /// true (only set by <see cref="OnFormClosingHandler"/>), blocks until
    /// the current tick + any background retention task have finished so
    /// the caller can safely dispose <see cref="db"/> / <see cref="ocr"/>
    /// without an ObjectDisposedException tearing down the process.
    /// </summary>
    private void Stop(bool waitForInflight = false)
    {
        try
        {
            // Cancel first so an in-flight tick observes cancellation ASAP.
            try { cts?.Cancel(); } catch { }

            if (timer != null)
            {
                if (waitForInflight)
                {
                    // Dispose(WaitHandle) signals when no callbacks are running.
                    using var done = new ManualResetEvent(false);
                    try { timer.Dispose(done); done.WaitOne(TimeSpan.FromSeconds(5)); }
                    catch { try { timer.Dispose(); } catch { } }
                }
                else
                {
                    timer.Dispose();
                }
                timer = null;
            }

            if (waitForInflight)
            {
                // Spin briefly until the captureBusy flag clears. Bounded so
                // a wedged OCR call can't keep the close hanging.
                var deadline = DateTime.UtcNow.AddSeconds(5);
                while (Interlocked.CompareExchange(ref captureBusy, 0, 0) == 1 && DateTime.UtcNow < deadline)
                    Thread.Sleep(50);

                // Same for the retention background task.
                try { lastRetentionTask?.Wait(TimeSpan.FromSeconds(5)); }
                catch { /* aggregate of cancelled / observed elsewhere */ }
            }
        }
        catch { }

        SetCaptureRunningState(false);
        UpdateTrayMenuState(false);
        SetStatus("");
        LogSink.Log("Stopped.");
    }

    private async Task TickAsync()
    {
        if (shuttingDown) return;
        if (Interlocked.Exchange(ref captureBusy, 1) == 1)
        {
            LogSink.Log("Skipping tick — previous capture still running.");
            return;
        }
        try
        {
            // Snapshot to locals — these can be nulled out by DisposeServices()
            // on another thread mid-tick. Working with locals keeps the rest
            // of the method safe from torn reads / use-after-dispose.
            var svc = service;
            if (svc == null) return;
            var ct = cts?.Token ?? CancellationToken.None;
            var result = await svc.CaptureOnceAsync(ct);
            if (shuttingDown) return;
            var snapshotLabel = result.SnapshotId > 0 ? $"snapshot #{result.SnapshotId}" : "no-change tick";
            LogSink.Log($"{snapshotLabel}: {result.StoredWindowCount}/{result.WindowCount} windows stored, {result.SkippedUnchanged} unchanged skipped, {result.ImageBytes / 1024} KB JPEG, {result.ElapsedMs} ms");
            try
            {
                var lastLine = $"Last: {DateTime.Now:HH:mm:ss}  ·  {result.StoredWindowCount}/{result.WindowCount} stored";
                if (!shuttingDown && IsHandleCreated)
                {
                    if (InvokeRequired) BeginInvoke(new Action(() => { if (!shuttingDown) capLastLbl.Text = lastLine; }));
                    else capLastLbl.Text = lastLine;
                }
            }
            catch { }
            UpdateDbLabel();
            browsePanel.InvalidateData();
            // Marshal a refresh to the UI thread. RefreshIfNeeded() itself
            // guards against running while the panel is hidden, the search
            // box is focused, or a previous refresh is still in flight.
            try
            {
                if (!shuttingDown && IsHandleCreated)
                {
                    if (InvokeRequired) BeginInvoke(new Action(() => { if (!shuttingDown) browsePanel.RefreshIfNeeded(); }));
                    else browsePanel.RefreshIfNeeded();
                }
            }
            catch { /* form closing race — harmless */ }
            RunRetentionSweep(force: false);
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { /* services disposed mid-tick during shutdown */ }
        catch (Exception ex) { LogSink.Log("Tick error: " + ex.Message); }
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

        lastRetentionTask = Task.Run(() =>
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
                    try { snapshotSettings.Save(); } catch (Exception ex) { LogSink.Log("[retention] compaction timestamp save error: " + ex.Message); }
                }

                if (r.Changed || didCompact)
                {
                    var compactMsg = didCompact
                        ? $", compacted DB ({FormatBytes(compaction.ReclaimedBytes)} reclaimed)"
                        : "";
                    LogSink.Log($"[retention] purged {r.RowsDeleted} row(s), stripped {r.ImagesPurged} image(s){compactMsg}.");
                    if (!shuttingDown && IsHandleCreated)
                    {
                        try
                        {
                            BeginInvoke(new Action(() =>
                            {
                                if (shuttingDown) return;
                                UpdateDbLabel();
                                browsePanel.InvalidateData();
                            }));
                        }
                        catch { /* form closing race */ }
                    }
                }
            }
            catch (ObjectDisposedException) { /* shutdown race */ }
            catch (Exception ex)
            {
                LogSink.Log("[retention] error: " + ex.Message);
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
            LogSink.Log("[clear] no database attached.");
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
                    catch (Exception ex) { LogSink.Log("[clear] compaction timestamp save error: " + ex.Message); }
                }
                LogSink.Log($"[clear] database wiped: {deleted} snapshot(s) deleted, compacted DB ({FormatBytes(compacted.ReclaimedBytes)} reclaimed).");
                BeginInvoke(new Action(() =>
                {
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
            Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
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
        LogSink.Log("ERROR: " + msg);
        MessageBox.Show(this, msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>Human-readable label for an <see cref="EncryptionMode"/> — used in the title bar and re-encryption dialog.</summary>
    private static string DescribeEncryption(EncryptionMode mode) => mode switch
    {
        EncryptionMode.None        => "None",
        EncryptionMode.UserAccount => "User Account",
        EncryptionMode.Passphrase  => "Password",
        _                          => mode.ToString(),
    };

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
