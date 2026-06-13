Add-Type -AssemblyName System.Drawing

function Draw-Icon([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic

    # --- rounded-square plate ---
    $r = [single]($size * 0.22)
    $d = $r * 2
    $plate = New-Object System.Drawing.Drawing2D.GraphicsPath
    $plate.AddArc(0, 0, $d, $d, 180, 90)
    $plate.AddArc($size - $d, 0, $d, $d, 270, 90)
    $plate.AddArc($size - $d, $size - $d, $d, $d, 0, 90)
    $plate.AddArc(0, $size - $d, $d, $d, 90, 90)
    $plate.CloseFigure()

    $rect = New-Object System.Drawing.RectangleF(0, 0, $size, $size)
    $plateBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        $rect,
        [System.Drawing.Color]::FromArgb(99, 80, 240),
        [System.Drawing.Color]::FromArgb(40, 200, 230),
        45.0)
    $g.FillPath($plateBrush, $plate)

    # --- infinity-brain: two loops forming a figure-8 ---
    # Scale of the unit design (256x256 reference) down to $size
    $s = [single]($size / 256.0)
    $stroke = [single]([Math]::Max(2, [Math]::Round(22 * $s)))

    $loops = New-Object System.Drawing.Drawing2D.GraphicsPath

    # Right loop: M 128 128  C 152 72, 224 72, 224 128  C 224 184, 152 184, 128 128
    $loops.AddBezier(
        128 * $s, 128 * $s,
        152 * $s,  72 * $s,
        224 * $s,  72 * $s,
        224 * $s, 128 * $s)
    $loops.AddBezier(
        224 * $s, 128 * $s,
        224 * $s, 184 * $s,
        152 * $s, 184 * $s,
        128 * $s, 128 * $s)

    # Left loop:  M 128 128  C 104 72, 32 72, 32 128  C 32 184, 104 184, 128 128
    $loops.StartFigure()
    $loops.AddBezier(
        128 * $s, 128 * $s,
        104 * $s,  72 * $s,
         32 * $s,  72 * $s,
         32 * $s, 128 * $s)
    $loops.AddBezier(
         32 * $s, 128 * $s,
         32 * $s, 184 * $s,
        104 * $s, 184 * $s,
        128 * $s, 128 * $s)

    $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, $stroke)
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $g.DrawPath($pen, $loops)

    # --- spark at the crossover (only above 24px; tiny dot below) ---
    if ($size -ge 32) {
        $glowR = [single](26 * $s)
        $gp = New-Object System.Drawing.Drawing2D.GraphicsPath
        $gp.AddEllipse(128 * $s - $glowR, 128 * $s - $glowR, $glowR * 2, $glowR * 2)
        $glow = New-Object System.Drawing.Drawing2D.PathGradientBrush($gp)
        $glow.CenterColor = [System.Drawing.Color]::FromArgb(255, 255, 244, 176)
        $glow.SurroundColors = @([System.Drawing.Color]::FromArgb(0, 255, 214, 80))
        $g.FillPath($glow, $gp)
        $glow.Dispose()
        $gp.Dispose()
    }
    $dotR = [single]([Math]::Max(1.5, 9 * $s))
    $dotBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 244, 176))
    $g.FillEllipse($dotBrush, 128 * $s - $dotR, 128 * $s - $dotR, $dotR * 2, $dotR * 2)

    $dotBrush.Dispose()
    $pen.Dispose()
    $plateBrush.Dispose()
    $plate.Dispose()
    $loops.Dispose()
    $g.Dispose()
    return $bmp
}

function Save-Png([System.Drawing.Bitmap]$bmp, [string]$path) {
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
}

function Bitmap-ToPngBytes([System.Drawing.Bitmap]$bmp) {
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    # wrap to prevent PowerShell from unrolling byte[] when returned
    return ,$ms.ToArray()
}

# --- generate all sizes ---
$sizes = @(16, 24, 32, 48, 64, 128, 256)
$entries = @()
foreach ($sz in $sizes) {
    $bmp = Draw-Icon $sz
    $entries += [pscustomobject]@{ Size = $sz; Data = (Bitmap-ToPngBytes $bmp) }
    if ($sz -eq 256) {
        Save-Png $bmp (Join-Path $PSScriptRoot 'app-icon-preview.png')
    }
    $bmp.Dispose()
}

# --- pack ICO ---
$out = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($out)
$bw.Write([UInt16]0)            # reserved
$bw.Write([UInt16]1)            # type = icon
$bw.Write([UInt16]$entries.Count)

$offset = 6 + (16 * $entries.Count)
foreach ($e in $entries) {
    $w = if ($e.Size -ge 256) { 0 } else { $e.Size }
    $h = if ($e.Size -ge 256) { 0 } else { $e.Size }
    $bw.Write([byte]$w)
    $bw.Write([byte]$h)
    $bw.Write([byte]0)          # palette colors
    $bw.Write([byte]0)          # reserved
    $bw.Write([UInt16]1)        # planes
    $bw.Write([UInt16]32)       # bpp
    $bw.Write([UInt32]$e.Data.Length)
    $bw.Write([UInt32]$offset)
    $offset += $e.Data.Length
}
foreach ($e in $entries) {
    [byte[]]$data = $e.Data
    $out.Write($data, 0, $data.Length)
}
$bw.Flush()

$icoPath = Join-Path $PSScriptRoot 'app.ico'
[System.IO.File]::WriteAllBytes($icoPath, $out.ToArray())
Write-Host ("wrote {0} ({1} bytes, {2} sizes)" -f $icoPath, $out.Length, $entries.Count)

# also copy preview to workspace for inline display
$ws = 'C:\Users\ifain\OneDrive - Microsoft\Documents\Microsoft Scout\app-icon-preview.png'
Copy-Item (Join-Path $PSScriptRoot 'app-icon-preview.png') $ws -Force
Write-Host ("preview at {0}" -f $ws)
