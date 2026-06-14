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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// A scrollable image viewer that supports a "fit-to-window" mode and discrete
/// zoom levels (25, 50, 75, 100, 150, 200%). Hosts a single PictureBox sized
/// either to the client area (Fit) or to the natural image size scaled by the
/// current zoom percent (Percent).
/// </summary>
public sealed class ZoomablePicturePanel : Panel
{
    // Discrete zoom steps we expose. Fit is signalled by ZoomPercent == 0.
    public static readonly int[] ZoomSteps = { 25, 50, 75, 100, 150, 200 };

    private readonly PictureBox pic;
    private Image? image;
    private int zoomPercent;     // 0 == Fit
    private bool inLayout;

    public event EventHandler? ZoomChanged;

    public ZoomablePicturePanel()
    {
        DoubleBuffered = true;
        AutoScroll = true;
        BackColor = Color.FromArgb(245, 245, 247);
        Padding = new Padding(0);

        pic = new PictureBox
        {
            BackColor = Color.FromArgb(245, 245, 247),
            BorderStyle = BorderStyle.None,
            SizeMode = PictureBoxSizeMode.Zoom,
            TabStop = false,
        };
        Controls.Add(pic);

        // Ctrl+wheel zoom; without Ctrl, let the panel scroll normally.
        MouseWheel += OnMouseWheel;
        pic.MouseWheel += OnMouseWheel;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? Image
    {
        get => image;
        set
        {
            image = value;
            pic.Image = value;
            ApplyLayout();
        }
    }

    /// <summary>0 == fit-to-window; otherwise one of <see cref="ZoomSteps"/>.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int ZoomPercent
    {
        get => zoomPercent;
        set
        {
            var clamped = value <= 0 ? 0 : Math.Clamp(value, ZoomSteps[0], ZoomSteps[^1]);
            if (clamped == zoomPercent) return;
            zoomPercent = clamped;
            ApplyLayout();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsFit => zoomPercent == 0;

    public void FitToWindow() => ZoomPercent = 0;
    public void OneToOne()    => ZoomPercent = 100;

    public void ZoomIn()
    {
        // Fit → 100% feels right as the first step out of Fit.
        if (IsFit) { ZoomPercent = 100; return; }
        for (int i = 0; i < ZoomSteps.Length; i++)
        {
            if (ZoomSteps[i] > zoomPercent) { ZoomPercent = ZoomSteps[i]; return; }
        }
        ZoomPercent = ZoomSteps[^1];
    }

    public void ZoomOut()
    {
        if (IsFit) return;
        for (int i = ZoomSteps.Length - 1; i >= 0; i--)
        {
            if (ZoomSteps[i] < zoomPercent) { ZoomPercent = ZoomSteps[i]; return; }
        }
        // Below the smallest step → back to Fit.
        ZoomPercent = 0;
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        if (IsFit) ApplyLayout();
    }

    private void OnMouseWheel(object? sender, MouseEventArgs e)
    {
        if ((ModifierKeys & Keys.Control) != Keys.Control) return;
        if (e.Delta > 0) ZoomIn(); else ZoomOut();
        if (sender is HandledMouseEventArgs h) h.Handled = true;
    }

    private void ApplyLayout()
    {
        if (inLayout) return;
        inLayout = true;
        try
        {
            if (image == null)
            {
                pic.SizeMode = PictureBoxSizeMode.Zoom;
                pic.Location = new Point(0, 0);
                pic.Size = ClientSize;
                AutoScrollMinSize = Size.Empty;
                return;
            }

            if (IsFit)
            {
                AutoScrollMinSize = Size.Empty;
                pic.SizeMode = PictureBoxSizeMode.Zoom;
                pic.Location = new Point(0, 0);
                pic.Size = ClientSize;
            }
            else
            {
                var w = Math.Max(1, image.Width * zoomPercent / 100);
                var h = Math.Max(1, image.Height * zoomPercent / 100);
                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                pic.Size = new Size(w, h);
                AutoScrollMinSize = pic.Size;
                // Center the picture in the panel when it's smaller than the viewport.
                var cw = ClientSize.Width;
                var ch = ClientSize.Height;
                var x = w < cw ? (cw - w) / 2 : 0;
                var y = h < ch ? (ch - h) / 2 : 0;
                pic.Location = new Point(x + AutoScrollPosition.X, y + AutoScrollPosition.Y);
            }
            pic.Invalidate();
        }
        finally { inLayout = false; }
    }
}
