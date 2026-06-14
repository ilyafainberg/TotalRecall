# Renders TotalRecall MSIX tile PNGs from scratch using System.Drawing,
# replicating TotalRecall\app.svg (gradient plate + figure-8 + spark).
Add-Type -AssemblyName System.Drawing

$AssetsDir = Join-Path $PSScriptRoot "Assets"
if (-not (Test-Path $AssetsDir)) { New-Item -ItemType Directory -Path $AssetsDir -Force | Out-Null }

# Colors from app.svg
$GradTL  = [System.Drawing.Color]::FromArgb(0xFF, 0x63, 0x50, 0xF0)  # #6350F0
$GradBR  = [System.Drawing.Color]::FromArgb(0xFF, 0x28, 0xC8, 0xE6)  # #28C8E6
$Stroke  = [System.Drawing.Color]::FromArgb(0xFF, 0xFF, 0xFF, 0xFF)  # white
$SparkIn = [System.Drawing.Color]::FromArgb(0xFF, 0xFF, 0xF4, 0xB0)  # #FFF4B0

function New-Tile {
    param(
        [int]$W,
        [int]$H,
        [string]$OutPath,
        [bool]$Transparent = $false
    )

    $bmp = New-Object System.Drawing.Bitmap($W, $H, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g   = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode      = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode  = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode    = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

    if ($Transparent) {
        $g.Clear([System.Drawing.Color]::Transparent)
    }

    # Icon is square — for wide/splash images we center a square plate.
    $plateSize = [Math]::Min($W, $H)
    $plateX = [int](($W - $plateSize) / 2)
    $plateY = [int](($H - $plateSize) / 2)

    # Corner radius scaled from 256-canvas (rx=56 → ratio 0.21875).
    $radius = [int]($plateSize * 0.21875)

    # Build rounded-square path
    $rect = New-Object System.Drawing.Rectangle($plateX, $plateY, $plateSize, $plateSize)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $radius * 2
    $path.AddArc($rect.X, $rect.Y, $d, $d, 180, 90)
    $path.AddArc($rect.Right - $d, $rect.Y, $d, $d, 270, 90)
    $path.AddArc($rect.Right - $d, $rect.Bottom - $d, $d, $d, 0, 90)
    $path.AddArc($rect.X, $rect.Bottom - $d, $d, $d, 90, 90)
    $path.CloseFigure()

    # Linear gradient (top-left → bottom-right)
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Point($rect.X, $rect.Y)),
        (New-Object System.Drawing.Point($rect.Right, $rect.Bottom)),
        $GradTL, $GradBR
    )
    $g.FillPath($brush, $path)
    $brush.Dispose()

    # Draw figure-8 — coordinates scaled from 256-canvas, anchored to plate.
    function ScaleX([double]$x) { return $plateX + ($x / 256.0) * $plateSize }
    function ScaleY([double]$y) { return $plateY + ($y / 256.0) * $plateSize }
    function ScaleS([double]$v) { return ($v / 256.0) * $plateSize }

    $strokeWidth = ScaleS 22
    $pen = New-Object System.Drawing.Pen($Stroke, $strokeWidth)
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap   = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

    # Right loop: M128 128 C 152 72, 224 72, 224 128 C 224 184, 152 184, 128 128 Z
    $right = New-Object System.Drawing.Drawing2D.GraphicsPath
    $right.AddBezier(
        (ScaleX 128), (ScaleY 128),
        (ScaleX 152), (ScaleY 72),
        (ScaleX 224), (ScaleY 72),
        (ScaleX 224), (ScaleY 128)
    )
    $right.AddBezier(
        (ScaleX 224), (ScaleY 128),
        (ScaleX 224), (ScaleY 184),
        (ScaleX 152), (ScaleY 184),
        (ScaleX 128), (ScaleY 128)
    )
    $g.DrawPath($pen, $right)
    $right.Dispose()

    # Left loop: M128 128 C 104 72, 32 72, 32 128 C 32 184, 104 184, 128 128 Z
    $left = New-Object System.Drawing.Drawing2D.GraphicsPath
    $left.AddBezier(
        (ScaleX 128), (ScaleY 128),
        (ScaleX 104), (ScaleY 72),
        (ScaleX 32),  (ScaleY 72),
        (ScaleX 32),  (ScaleY 128)
    )
    $left.AddBezier(
        (ScaleX 32),  (ScaleY 128),
        (ScaleX 32),  (ScaleY 184),
        (ScaleX 104), (ScaleY 184),
        (ScaleX 128), (ScaleY 128)
    )
    $g.DrawPath($pen, $left)
    $left.Dispose()
    $pen.Dispose()

    # Spark — small solid yellow dot at crossover.
    $sparkR  = [int](ScaleS 9)
    $sparkBr = New-Object System.Drawing.SolidBrush($SparkIn)
    $cx = [int](ScaleX 128)
    $cy = [int](ScaleY 128)
    $g.FillEllipse($sparkBr, $cx - $sparkR, $cy - $sparkR, $sparkR * 2, $sparkR * 2)
    $sparkBr.Dispose()

    $g.Dispose()
    $bmp.Save($OutPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "  ✓ $OutPath ($W x $H)"
}

# Required MSIX tile assets
$tiles = @(
    @{ Name = "Square44x44Logo.png";    W =  44; H =  44 },
    @{ Name = "Square71x71Logo.png";    W =  71; H =  71 },
    @{ Name = "Square150x150Logo.png";  W = 150; H = 150 },
    @{ Name = "Square310x310Logo.png";  W = 310; H = 310 },
    @{ Name = "Wide310x150Logo.png";    W = 310; H = 150 },
    @{ Name = "StoreLogo.png";          W =  50; H =  50 },
    @{ Name = "SplashScreen.png";       W = 620; H = 300 }
)

Write-Host "Rendering MSIX tiles to $AssetsDir"
foreach ($t in $tiles) {
    New-Tile -W $t.W -H $t.H -OutPath (Join-Path $AssetsDir $t.Name)
}
Write-Host "Done. $($tiles.Count) tiles rendered."
