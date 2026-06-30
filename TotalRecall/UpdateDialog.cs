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
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// Modal "Check for updates" dialog. Queries GitHub for the latest release, and — when a
/// newer build exists — downloads the portable archive with a live progress bar, then
/// reveals it in Explorer so the user can swap it in.
/// </summary>
public sealed class UpdateDialog : Form
{
    private readonly Version current;
    private readonly Label statusLbl;
    private readonly Label detailLbl;
    private readonly ProgressBar progress;
    private readonly Button actionBtn;
    private readonly Button closeBtn;
    private readonly LinkLabel releaseLink;
    private readonly CancellationTokenSource cts = new();

    private UpdateInfo? available;
    private string? downloadedPath;
    private bool downloading;

    public UpdateDialog() : this(Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0)) { }

    public UpdateDialog(Version current)
    {
        this.current = current;

        Text = "TotalRecall — Updates";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Theme.BgPanel;
        ForeColor = Theme.Fg;
        Font = Theme.UiFont;
        ClientSize = new Size(460, 220);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        statusLbl = new Label
        {
            Text = "Checking for updates…",
            Font = new Font("Segoe UI Semibold", 11f),
            ForeColor = Theme.Fg,
            AutoSize = true,
            Location = new Point(24, 24),
        };

        detailLbl = new Label
        {
            Text = $"Current version: {current.ToString(3)}",
            ForeColor = Theme.FgMuted,
            AutoSize = true,
            MaximumSize = new Size(412, 0),
            Location = new Point(24, 58),
        };

        releaseLink = new LinkLabel
        {
            Text = "View release notes",
            AutoSize = true,
            Visible = false,
            LinkColor = Theme.Accent,
            ActiveLinkColor = Theme.AccentHover,
            Location = new Point(24, 86),
        };
        releaseLink.LinkClicked += (_, _) =>
        {
            if (available != null) OpenUrl(available.ReleaseUrl);
        };

        progress = new ProgressBar
        {
            Location = new Point(24, 120),
            Size = new Size(412, 22),
            Style = ProgressBarStyle.Continuous,
            Minimum = 0,
            Maximum = 100,
            Visible = false,
        };

        actionBtn = new Button
        {
            Text = "Download",
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Accent,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Size = new Size(150, 34),
            Location = new Point(24, ClientSize.Height - 50),
            Visible = false,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
        };
        actionBtn.FlatAppearance.BorderColor = Theme.Border;
        actionBtn.Click += async (_, _) => await OnActionAsync();

        closeBtn = new Button
        {
            Text = "Close",
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.BgRaised,
            ForeColor = Theme.Fg,
            Cursor = Cursors.Hand,
            Size = new Size(110, 34),
            Location = new Point(ClientSize.Width - 134, ClientSize.Height - 50),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };
        closeBtn.FlatAppearance.BorderColor = Theme.Border;
        closeBtn.Click += (_, _) => Close();

        Controls.Add(statusLbl);
        Controls.Add(detailLbl);
        Controls.Add(releaseLink);
        Controls.Add(progress);
        Controls.Add(actionBtn);
        Controls.Add(closeBtn);

        CancelButton = closeBtn;
        Shown += async (_, _) => await CheckAsync();
        FormClosing += (_, e) =>
        {
            if (downloading)
            {
                // Don't tear down mid-download; cancel and let it unwind.
                cts.Cancel();
            }
        };
    }

    private async Task CheckAsync()
    {
        try
        {
            var info = await UpdateService.CheckForUpdateAsync(current, cts.Token);
            if (cts.IsCancellationRequested) return;

            if (info == null)
            {
                statusLbl.Text = "You're up to date.";
                detailLbl.Text = $"TotalRecall {current.ToString(3)} is the latest version.";
                return;
            }

            available = info;
            statusLbl.Text = $"Update available: v{info.Version.ToString(3)}";
            var size = info.SizeBytes > 0 ? $"  ·  {FormatBytes(info.SizeBytes)}" : "";
            detailLbl.Text = $"You have {current.ToString(3)}.  Download {info.AssetName}{size}.";
            releaseLink.Visible = !string.IsNullOrEmpty(info.ReleaseUrl);
            actionBtn.Visible = true;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            statusLbl.Text = "Couldn't check for updates.";
            detailLbl.Text = ex.Message;
        }
    }

    private async Task OnActionAsync()
    {
        // Second click (after a successful download) reveals the file in Explorer.
        if (downloadedPath != null)
        {
            RevealInExplorer(downloadedPath);
            return;
        }
        if (available == null) return;

        downloading = true;
        actionBtn.Enabled = false;
        closeBtn.Enabled = false;
        progress.Visible = true;
        progress.Style = ProgressBarStyle.Marquee;
        statusLbl.Text = "Downloading…";

        var reporter = new Progress<double>(p =>
        {
            if (p < 0)
            {
                progress.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                progress.Style = ProgressBarStyle.Continuous;
                progress.Value = Math.Clamp((int)(p * 100), 0, 100);
                statusLbl.Text = $"Downloading… {progress.Value}%";
            }
        });

        try
        {
            var path = await UpdateService.DownloadAsync(available, reporter, cts.Token);
            downloadedPath = path;
            progress.Style = ProgressBarStyle.Continuous;
            progress.Value = 100;
            statusLbl.Text = "Download complete.";
            detailLbl.Text = "Extract the archive and replace your existing TotalRecall files, then relaunch.";
            actionBtn.Text = "Show in folder";
            actionBtn.Enabled = true;
        }
        catch (OperationCanceledException)
        {
            statusLbl.Text = "Download cancelled.";
            actionBtn.Enabled = true;
        }
        catch (Exception ex)
        {
            statusLbl.Text = "Download failed.";
            detailLbl.Text = ex.Message;
            actionBtn.Enabled = true;
        }
        finally
        {
            downloading = false;
            closeBtn.Enabled = true;
        }
    }

    private static void RevealInExplorer(string path)
    {
        try { Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true }); }
        catch { /* explorer missing/blocked — nothing actionable */ }
    }

    private static void OpenUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); }
        catch { }
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double v = bytes; int u = 0;
        while (v >= 1024 && u < units.Length - 1) { v /= 1024; u++; }
        return $"{v:0.##} {units[u]}";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) cts.Dispose();
        base.Dispose(disposing);
    }
}
