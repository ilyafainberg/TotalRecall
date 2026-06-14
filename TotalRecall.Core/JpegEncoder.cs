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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace TotalRecall;

/// <summary>
/// Encodes a <see cref="Bitmap"/> as a JPEG byte array. JPEG has no alpha channel,
/// so 32-bit ARGB inputs are first flattened onto a white background — leaving them
/// in place would emit black or random pixels for the previously-transparent regions.
/// </summary>
public static class JpegEncoder
{
    private static readonly ImageCodecInfo s_jpegCodec =
        ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");

    /// <summary>
    /// Encodes <paramref name="bmp"/> as a JPEG. <paramref name="quality"/> is clamped
    /// to [30, 95]; the GDI+ JPEG encoder's curve flattens out above 95 (file size
    /// explodes for negligible quality gain) and falls apart below 30.
    /// </summary>
    public static byte[] Encode(Bitmap bmp, int quality)
    {
        quality = Math.Clamp(quality, 30, 95);
        using var ms = new MemoryStream();
        var ep = new EncoderParameters(1);
        ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

        // Flatten to white background (JPEG has no alpha).
        if (bmp.PixelFormat == PixelFormat.Format32bppArgb || bmp.PixelFormat == PixelFormat.Format32bppPArgb)
        {
            using var flat = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(flat))
            {
                g.Clear(Color.White);
                g.DrawImageUnscaled(bmp, 0, 0);
            }
            flat.Save(ms, s_jpegCodec, ep);
        }
        else
        {
            bmp.Save(ms, s_jpegCodec, ep);
        }
        return ms.ToArray();
    }
}
