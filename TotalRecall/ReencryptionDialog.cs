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
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// Modal "Re-encrypting database…" dialog. Runs <see cref="Database.Rekey"/> on a
/// background thread and polls the destination temp file's size so the user sees
/// real progress for big DBs. While the rekey is in flight the parent window is
/// blocked (modal), so the only way to interact with the app is the dialog's
/// Quit button. A confirmation prompt warns the user that re-encryption keeps
/// running in the background after Quit and the app will exit when it finishes;
/// shutting the computer down or putting it to sleep mid-rekey can corrupt the
/// database, so we hold the process alive until <see cref="Database.Rekey"/>
/// returns even after the user has clicked Quit.
/// </summary>
internal sealed class ReencryptionDialog : Form
{
    private readonly string dbPath;
    private readonly string tempPath;
    private readonly string? oldKey;
    private readonly string? newKey;
    private readonly long sourceBytes;

    private readonly Label titleLbl = new();
    private readonly Label subtitleLbl = new();
    private readonly Label statusLbl = new();
    private readonly ProgressBar bar = new();
    private readonly Button quitBtn = new();
    private readonly System.Windows.Forms.Timer pollTimer = new() { Interval = 250 };

    private Task? worker;
    private volatile bool completed;
    private volatile bool quitRequested;
    private Exception? error;

    public bool QuitRequested => quitRequested;
    public Exception? Error => error;

    public ReencryptionDialog(string dbPath, string? oldKey, string? newKey, string fromLabel, string toLabel)
    {
        this.dbPath = dbPath;
        this.oldKey = oldKey;
        this.newKey = newKey;
        tempPath = dbPath + ".rekey-tmp";
        try { sourceBytes = new FileInfo(dbPath).Length; } catch { sourceBytes = 0; }

        Text = "TotalRecall — Re-encrypting database";
        BackColor = Theme.Bg;
        ForeColor = Theme.Fg;
        Font = Theme.UiFont;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ControlBox = false;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        KeyPreview = true;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(520, 220);

        // Title
        titleLbl.AutoSize = false;
        titleLbl.Text = "Re-encrypting database";
        titleLbl.Font = new Font("Segoe UI Semibold", 13f);
        titleLbl.ForeColor = Theme.Fg;
        titleLbl.Location = new Point(24, 22);
        titleLbl.Size = new Size(480, 26);

        // Subtitle (from → to)
        subtitleLbl.AutoSize = false;
        subtitleLbl.Text = $"Switching encryption: {fromLabel} → {toLabel}";
        subtitleLbl.ForeColor = Theme.FgMuted;
        subtitleLbl.Font = Theme.UiFont;
        subtitleLbl.Location = new Point(24, 50);
        subtitleLbl.Size = new Size(480, 20);

        // Progress bar
        bar.Style = ProgressBarStyle.Continuous;
        bar.Minimum = 0;
        bar.Maximum = 100;
        bar.Value = 0;
        bar.Location = new Point(24, 90);
        bar.Size = new Size(472, 18);

        // Status line under the bar
        statusLbl.AutoSize = false;
        statusLbl.Text = "Preparing…";
        statusLbl.ForeColor = Theme.FgMuted;
        statusLbl.Font = Theme.UiFont;
        statusLbl.Location = new Point(24, 116);
        statusLbl.Size = new Size(472, 38);

        // Quit button
        quitBtn.Text = "Quit";
        quitBtn.BackColor = Color.White;
        quitBtn.ForeColor = Theme.Fg;
        quitBtn.FlatStyle = FlatStyle.Flat;
        quitBtn.FlatAppearance.BorderColor = Theme.Border;
        quitBtn.Font = new Font("Segoe UI Semibold", 9.5f);
        quitBtn.Cursor = Cursors.Hand;
        quitBtn.Size = new Size(110, 32);
        quitBtn.Location = new Point(ClientSize.Width - quitBtn.Width - 24, ClientSize.Height - quitBtn.Height - 18);
        quitBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        quitBtn.Click += (_, _) => OnQuitClicked();

        Controls.AddRange(new Control[] { titleLbl, subtitleLbl, bar, statusLbl, quitBtn });

        pollTimer.Tick += OnPollTick;
        // Swallow Esc — the only way out is via Quit or completion.
        KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) e.Handled = true; };
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        LogSink.Log($"[rekey] dialog shown — source={FormatBytes(sourceBytes)}, dest='{tempPath}'");
        // Kick off the rekey on a background thread. Database.Rekey is fully
        // synchronous — we let it run to completion and surface any exception
        // via the Error property on the UI thread.
        worker = Task.Run(() =>
        {
            try
            {
                LogSink.Log("[rekey] worker started");
                Database.Rekey(dbPath, oldKey, newKey);
                LogSink.Log("[rekey] worker finished OK");
            }
            catch (Exception ex)
            {
                error = ex;
                LogSink.Log("[rekey] worker FAILED: " + ex);
            }
            finally { completed = true; }
        });
        pollTimer.Start();
    }

    private void OnPollTick(object? sender, EventArgs e)
    {
        // Estimate progress from the growing temp file. sqlcipher_export writes
        // pages to it sequentially, so size / sourceBytes is a good linear
        // approximation. Final 1% is reserved for the close-and-swap phase
        // which doesn't add bytes — we jump straight to 100 on completion.
        long tempSize = 0;
        try { if (File.Exists(tempPath)) tempSize = new FileInfo(tempPath).Length; }
        catch { /* OneDrive can race on Length — best-effort */ }

        if (!completed)
        {
            int pct = sourceBytes > 0
                ? (int)Math.Clamp(tempSize * 100L / sourceBytes, 0L, 99L)
                : (bar.Value < 90 ? bar.Value + 1 : 90);
            bar.Value = pct;
            statusLbl.Text = quitRequested
                ? $"Finishing… {FormatBytes(tempSize)} of {FormatBytes(sourceBytes)} written. The app will exit when this completes."
                : $"Copying… {FormatBytes(tempSize)} of {FormatBytes(sourceBytes)} ({pct}%)";
            return;
        }

        // Done: drive bar to 100, then close.
        pollTimer.Stop();
        bar.Value = 100;
        statusLbl.Text = error == null ? "Done." : "Failed.";
        DialogResult = error == null ? DialogResult.OK : DialogResult.Abort;
        Close();
    }

    private void OnQuitClicked()
    {
        if (completed)
        {
            DialogResult = error == null ? DialogResult.OK : DialogResult.Abort;
            Close();
            return;
        }

        var r = MessageBox.Show(this,
            "Re-encryption is still in progress.\r\n\r\n" +
            "If you quit now, re-encryption will keep running in the background " +
            "and TotalRecall will exit automatically when it's done. " +
            "Don't shut down or sleep your computer until you see the app close — " +
            "interrupting an in-flight rekey can corrupt the database.\r\n\r\n" +
            "Quit when re-encryption finishes?",
            "Quit during re-encryption",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

        if (r != DialogResult.Yes) return;

        quitRequested = true;
        quitBtn.Enabled = false;
        quitBtn.Text = "Quitting…";
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // The dialog can only close once the worker has finished. Any other
        // close attempt (Alt+F4, system menu — both nominally disabled, but
        // belt and braces) gets cancelled while the rekey is running.
        if (!completed)
        {
            e.Cancel = true;
            return;
        }
        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            pollTimer.Stop();
            pollTimer.Dispose();
        }
        base.Dispose(disposing);
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024L * 1024) return $"{bytes / 1024.0:0.0} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):0.0} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):0.00} GB";
    }
}
