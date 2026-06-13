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
using System.Drawing.Imaging;
using System.IO;
using Tesseract;

namespace TotalRecall;

public sealed class OcrService : IDisposable
{
    private readonly TesseractEngine engine;
    private readonly object syncLock = new();
    private readonly int maxDimension;

    public OcrService(string tessdataPath, string language = "eng", int maxDimension = 1600)
    {
        if (!Directory.Exists(tessdataPath))
            throw new DirectoryNotFoundException($"tessdata directory not found: {tessdataPath}");

        var trained = Path.Combine(tessdataPath, language + ".traineddata");
        if (!File.Exists(trained))
            throw new FileNotFoundException($"Missing language data file: {trained}");

        engine = new TesseractEngine(tessdataPath, language, EngineMode.Default);
        this.maxDimension = Math.Clamp(maxDimension, 400, 3840);
    }

    public string Recognize(Bitmap bmp)
    {
        using var normalized = NormalizeForOcr(bmp);
        // Hand Leptonica an uncompressed BMP built directly from the pixel buffer.
        // Avoids the old PNG encode (compression) and a second byte[] copy per frame.
        var bmpBytes = ToBmpBytes(normalized);
        lock (syncLock)
        {
            using var pix = Pix.LoadFromMemory(bmpBytes);
            using var page = engine.Process(pix);
            return (page.GetText() ?? "").Trim();
        }
    }

    /// <summary>
    /// Serializes a <see cref="PixelFormat.Format24bppRgb"/> bitmap into an in-memory
    /// uncompressed 24-bit BMP. GDI's native BGR row layout maps straight onto BMP, so
    /// this is a single allocation plus a per-row memcpy — no image codec involved.
    /// </summary>
    private static unsafe byte[] ToBmpBytes(Bitmap bmp)
    {
        int width = bmp.Width;
        int height = bmp.Height;
        var data = bmp.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        try
        {
            int srcStride = data.Stride;
            int rowBytes = width * 3;
            int dstStride = (rowBytes + 3) & ~3; // BMP rows are padded to 4 bytes.
            int pixelDataSize = dstStride * height;
            const int headerSize = 54;          // 14-byte file header + 40-byte DIB header.
            var bytes = new byte[headerSize + pixelDataSize];

            bytes[0] = (byte)'B';
            bytes[1] = (byte)'M';
            WriteInt32(bytes, 2, bytes.Length);       // total file size
            WriteInt32(bytes, 10, headerSize);        // offset to pixel data
            WriteInt32(bytes, 14, 40);                // DIB header size
            WriteInt32(bytes, 18, width);
            WriteInt32(bytes, 22, height);            // positive => bottom-up rows
            bytes[26] = 1;                            // color planes
            bytes[28] = 24;                           // bits per pixel
            WriteInt32(bytes, 34, pixelDataSize);

            byte* src = (byte*)data.Scan0;
            fixed (byte* dstBase = bytes)
            {
                byte* pixels = dstBase + headerSize;
                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = src + (long)y * srcStride;
                    byte* dstRow = pixels + (long)(height - 1 - y) * dstStride; // bottom-up
                    Buffer.MemoryCopy(srcRow, dstRow, rowBytes, rowBytes);
                }
            }
            return bytes;
        }
        finally
        {
            bmp.UnlockBits(data);
        }
    }

    private static void WriteInt32(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
        buffer[offset + 3] = (byte)(value >> 24);
    }

    private Bitmap NormalizeForOcr(Bitmap source)
    {
        var scale = Math.Min(1.0, (double)maxDimension / Math.Max(source.Width, source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));

        var normalized = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        using var g = Graphics.FromImage(normalized);
        g.Clear(Color.White);
        g.InterpolationMode = scale < 1.0
            ? System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            : System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix(new[]
        {
            new[] { .299f, .299f, .299f, 0, 0 },
            new[] { .587f, .587f, .587f, 0, 0 },
            new[] { .114f, .114f, .114f, 0, 0 },
            new[] { 0f,    0f,    0f,    1, 0 },
            new[] { 0f,    0f,    0f,    0, 1 },
        }));
        g.DrawImage(source, new Rectangle(0, 0, width, height),
            0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
        return normalized;
    }

    public void Dispose() => engine.Dispose();
}
