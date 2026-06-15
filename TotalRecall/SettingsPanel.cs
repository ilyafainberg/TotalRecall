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
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// UserControl that hosts every user-tunable knob: capture cadence, retention, OCR
/// thresholds, encryption, app exclusion list, auto-start, and the destructive
/// "purge / clear database" buttons.
/// </summary>
/// <remarks>
/// <para>This control is shared between the in-window Settings flow and the standalone
/// <see cref="SettingsForm"/> wrapper. It raises three events the host listens for:</para>
/// <list type="bullet">
///   <item><see cref="SettingsSaved"/> — user clicked Save; host should call
///     <see cref="AppSettings.Save"/> and re-apply any runtime hot-changes (interval timer etc.).</item>
///   <item><see cref="PurgeRequested"/> — user clicked "Purge now"; host should run the retention sweep.</item>
///   <item><see cref="ClearDatabaseRequested"/> — user clicked "Clear database"; host should
///     run the nuclear delete-everything path.</item>
/// </list>
/// </remarks>
public partial class SettingsPanel : UserControl
{
    private AppSettings? settings;

    public event EventHandler? SettingsSaved;
    public event EventHandler? PurgeRequested;
    public event EventHandler? ClearDatabaseRequested;
    public event EventHandler? Cancelled;

    public SettingsPanel()
    {
        InitializeComponent();
        WireEvents();
    }

    public SettingsPanel(AppSettings settings) : this()
    {
        BindSettings(settings);
    }

    private void WireEvents()
    {
        qualityBar.ValueChanged += (_, _) => qualityValueLbl.Text = qualityBar.Value.ToString();
        dbBrowseBtn.Click += (_, _) => BrowseDb();
        encNoneRb.CheckedChanged += (_, _) => passTxt.Enabled = encPassRb.Checked;
        encUserRb.CheckedChanged += (_, _) => passTxt.Enabled = encPassRb.Checked;
        encPassRb.CheckedChanged += (_, _) => passTxt.Enabled = encPassRb.Checked;
        purgeImagesChk.CheckedChanged += (_, _) => purgeImagesNud.Enabled = purgeImagesChk.Checked;
        purgeAllChk.CheckedChanged += (_, _) => purgeAllNud.Enabled = purgeAllChk.Checked;
        compactAfterRetentionChk.CheckedChanged += (_, _) => compactAfterRetentionHoursNud.Enabled = compactAfterRetentionChk.Checked;
        purgeNowBtn.Click += (_, _) => PurgeRequested?.Invoke(this, EventArgs.Empty);
        clearDbBtn.Click += (_, _) => OnClearDatabaseClicked();
        saveBtn.Click += (_, _) => Save();
        cancelBtn.Click += (_, _) => Cancelled?.Invoke(this, EventArgs.Empty);
    }

    public void BindSettings(AppSettings settings)
    {
        this.settings = settings;
        intervalNud.Value = Math.Clamp(settings.IntervalSeconds, (int)intervalNud.Minimum, (int)intervalNud.Maximum);
        qualityBar.Value = Math.Clamp(settings.JpegQuality, qualityBar.Minimum, qualityBar.Maximum);
        qualityValueLbl.Text = qualityBar.Value.ToString();
        storeImagesChk.Checked = settings.StoreScreenshots;
        changeDetectionChk.Checked = settings.EnableChangeDetection;
        foregroundOnlyChk.Checked = settings.CaptureForegroundOnly;
        excludedAppsTxt.Text = settings.ExcludedApps;
        ocrMaxDimensionNud.Value = Math.Clamp(settings.OcrMaxDimension, (int)ocrMaxDimensionNud.Minimum, (int)ocrMaxDimensionNud.Maximum);
        dbPathTxt.Text = settings.DatabasePath;
        langCombo.Text = settings.OcrLanguage;
        switch (settings.EncryptionMode)
        {
            case EncryptionMode.None: encNoneRb.Checked = true; break;
            case EncryptionMode.UserAccount: encUserRb.Checked = true; break;
            case EncryptionMode.Passphrase: encPassRb.Checked = true; break;
        }
        passTxt.Enabled = encPassRb.Checked;
        startAtLoginChk.Checked = settings.StartAtLogin || AutoStart.IsEnabled();
        minToTrayChk.Checked = settings.MinimizeToTray;

        purgeImagesChk.Checked = settings.PurgeImagesEnabled;
        purgeImagesNud.Value = Math.Clamp(settings.PurgeImagesAfterDays, (int)purgeImagesNud.Minimum, (int)purgeImagesNud.Maximum);
        purgeImagesNud.Enabled = purgeImagesChk.Checked;
        purgeAllChk.Checked = settings.PurgeAllEnabled;
        purgeAllNud.Value = Math.Clamp(settings.PurgeAllAfterDays, (int)purgeAllNud.Minimum, (int)purgeAllNud.Maximum);
        purgeAllNud.Enabled = purgeAllChk.Checked;
        compactAfterRetentionChk.Checked = settings.CompactAfterRetentionEnabled;
        compactAfterRetentionHoursNud.Value = Math.Clamp(settings.CompactAfterRetentionHours, (int)compactAfterRetentionHoursNud.Minimum, (int)compactAfterRetentionHoursNud.Maximum);
        compactAfterRetentionHoursNud.Enabled = compactAfterRetentionChk.Checked;
    }

    private void BrowseDb()
    {
        using var dlg = new SaveFileDialog
        {
            FileName = Path.GetFileName(dbPathTxt.Text),
            InitialDirectory = Path.GetDirectoryName(dbPathTxt.Text) ?? AppSettings.AppDataDir,
            Filter = "SQLite database (*.db)|*.db|All files (*.*)|*.*",
            OverwritePrompt = false,
            Title = "Choose database file",
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
            dbPathTxt.Text = dlg.FileName;
    }

    private void OnClearDatabaseClicked()
    {
        var result = MessageBox.Show(this,
            "This will permanently delete ALL captured snapshots — screenshots, OCR text, " +
            "window metadata, and the full-text search index.\n\n" +
            "The database file itself is kept (and re-VACUUMed to reclaim disk space) " +
            "so capture can continue with a clean slate.\n\n" +
            "This cannot be undone. Continue?",
            "Clear database",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result == DialogResult.Yes)
            ClearDatabaseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void Save()
    {
        if (settings == null) return;

        settings.IntervalSeconds = (int)intervalNud.Value;
        settings.JpegQuality = qualityBar.Value;
        settings.StoreScreenshots = storeImagesChk.Checked;
        settings.EnableChangeDetection = changeDetectionChk.Checked;
        settings.CaptureForegroundOnly = foregroundOnlyChk.Checked;
        settings.ExcludedApps = excludedAppsTxt.Text.Trim();
        settings.OcrMaxDimension = (int)ocrMaxDimensionNud.Value;
        settings.DatabasePath = dbPathTxt.Text.Trim();
        settings.OcrLanguage = string.IsNullOrWhiteSpace(langCombo.Text) ? "eng" : langCombo.Text.Trim();

        EncryptionMode mode = EncryptionMode.None;
        if (encUserRb.Checked) mode = EncryptionMode.UserAccount;
        else if (encPassRb.Checked) mode = EncryptionMode.Passphrase;
        settings.EncryptionMode = mode;
        settings.RuntimePassphrase = mode == EncryptionMode.Passphrase ? passTxt.Text : null;

        settings.StartAtLogin = startAtLoginChk.Checked;
        settings.MinimizeToTray = minToTrayChk.Checked;

        settings.PurgeImagesEnabled = purgeImagesChk.Checked;
        settings.PurgeImagesAfterDays = (int)purgeImagesNud.Value;
        settings.PurgeAllEnabled = purgeAllChk.Checked;
        settings.PurgeAllAfterDays = (int)purgeAllNud.Value;
        settings.CompactAfterRetentionEnabled = compactAfterRetentionChk.Checked;
        settings.CompactAfterRetentionHours = (int)compactAfterRetentionHoursNud.Value;

        try
        {
            settings.Save();
            try { AutoStart.Set(settings.StartAtLogin); }
            catch (Exception regEx)
            {
                MessageBox.Show(this,
                    "Settings saved, but updating the Windows startup entry failed: " + regEx.Message,
                    "TotalRecall", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Failed to save settings: " + ex.Message, "TotalRecall", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void dbPathTxt_TextChanged(object sender, EventArgs e)
    {

    }
}
