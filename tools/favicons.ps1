param([string]$OutDir = (Join-Path $PSScriptRoot "../assets/img/favicons"))
# Renders favicon.ico (16, 32, 48), favicon-96x96.png and apple-touch-icon.png
# from the favicon mark, drawn with GDI+. assets/img/favicons/favicon.svg holds the
# same mark in 512 units; keep the two in step by hand when either changes.
# The 192 and 512 app icons and the avatar are the original artwork, not made here.
# Usage: powershell -ExecutionPolicy Bypass -File tools/favicons.ps1
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'

function C([string]$hex, [int]$a = 255) {
  $r = [Convert]::ToInt32($hex.Substring(1,2),16); $g = [Convert]::ToInt32($hex.Substring(3,2),16); $b = [Convert]::ToInt32($hex.Substring(5,2),16)
  return [System.Drawing.Color]::FromArgb($a, $r, $g, $b)
}

function Draw-Master([int]$px, [bool]$square) {
  $bmp = New-Object System.Drawing.Bitmap $px, $px, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'
  $g.Clear([System.Drawing.Color]::Transparent)
  $s = $px / 512.0
  $g.ScaleTransform($s, $s)

  # Sky: radial from a slightly green center to near-black.
  $disc = New-Object System.Drawing.Drawing2D.GraphicsPath
  if ($square) { $disc.AddRectangle((New-Object System.Drawing.RectangleF 0, 0, 512, 512)) } else { $disc.AddEllipse(0, 0, 512, 512) }
  if ($square) { $g.FillPath((New-Object System.Drawing.SolidBrush (C '#06080b')), $disc) }
  $skyPath = New-Object System.Drawing.Drawing2D.GraphicsPath
  $skyPath.AddEllipse(-25.6, -51.2, 563.2, 563.2)
  $sky = New-Object System.Drawing.Drawing2D.PathGradientBrush $skyPath
  $sky.CenterPoint = New-Object System.Drawing.PointF 256, 230
  $sky.CenterColor = C '#22341a'
  $sky.SurroundColors = @(C '#06080b')
  $g.SetClip($disc)
  $g.FillRectangle((New-Object System.Drawing.SolidBrush (C '#06080b')), 0, 0, 512, 512)
  $g.FillPath($sky, $skyPath)
  $g.ResetClip()

  # Glow behind the flare.
  $glowPath = New-Object System.Drawing.Drawing2D.GraphicsPath
  $glowPath.AddEllipse(86, 180, 340, 152)
  $glow = New-Object System.Drawing.Drawing2D.PathGradientBrush $glowPath
  $glow.CenterPoint = New-Object System.Drawing.PointF 256, 256
  $glow.CenterColor = C '#ff9a2a' 140
  $glow.SurroundColors = @(C '#ff7a00' 0)
  $g.FillPath($glow, $glowPath)

  # Flare: a pinched spindle with long tapered tips.
  $flare = New-Object System.Drawing.Drawing2D.GraphicsPath
  $flare.AddBezier(24, 256, 200, 252, 214, 190, 256, 190)
  $flare.AddBezier(256, 190, 298, 190, 312, 252, 488, 256)
  $flare.AddBezier(488, 256, 312, 260, 298, 322, 256, 322)
  $flare.AddBezier(256, 322, 214, 322, 200, 260, 24, 256)
  $flare.CloseFigure()
  $lin = New-Object System.Drawing.Drawing2D.LinearGradientBrush ((New-Object System.Drawing.PointF 23, 0), (New-Object System.Drawing.PointF 489, 0), (C '#ff7a00' 51), (C '#ff7a00' 51))
  $blend = New-Object System.Drawing.Drawing2D.ColorBlend 5
  $blend.Colors = @((C '#ff7a00' 51), (C '#ff8a14'), (C '#ffd15c'), (C '#ff8a14'), (C '#ff7a00' 51))
  $blend.Positions = @(0.0, 0.3, 0.5, 0.7, 1.0)
  $lin.InterpolationColors = $blend
  $g.FillPath($lin, $flare)

  # Core: white-hot ellipse fading out.
  $corePath = New-Object System.Drawing.Drawing2D.GraphicsPath
  $corePath.AddEllipse(184, 207, 144, 98)
  $core = New-Object System.Drawing.Drawing2D.PathGradientBrush $corePath
  $core.CenterPoint = New-Object System.Drawing.PointF 256, 256
  $cb = New-Object System.Drawing.Drawing2D.ColorBlend 3
  $cb.Colors = @((C '#ffd15c' 0), (C '#fff1b8'), (C '#ffffff'))
  $cb.Positions = @(0.0, 0.6, 1.0)
  $core.InterpolationColors = $cb
  $g.FillPath($core, $corePath)

  $g.Dispose()
  return $bmp
}

function Scale-To($src, [int]$px) {
  # Halve repeatedly, then a final resize, so small sizes stay clean.
  $cur = $src
  while ($cur.Width / 2 -ge $px * 2) {
    $half = New-Object System.Drawing.Bitmap ([int]($cur.Width / 2)), ([int]($cur.Height / 2)), ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $hg = [System.Drawing.Graphics]::FromImage($half); $hg.InterpolationMode = 'HighQualityBicubic'; $hg.PixelOffsetMode = 'HighQuality'; $hg.CompositingMode = 'SourceCopy'
    $hg.DrawImage($cur, 0, 0, $half.Width, $half.Height); $hg.Dispose()
    $cur = $half
  }
  $out = New-Object System.Drawing.Bitmap $px, $px, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $og = [System.Drawing.Graphics]::FromImage($out); $og.InterpolationMode = 'HighQualityBicubic'; $og.PixelOffsetMode = 'HighQuality'; $og.CompositingMode = 'SourceCopy'
  $og.DrawImage($cur, 0, 0, $px, $px); $og.Dispose()
  return $out
}

function Png-Bytes($bmp) { $ms = New-Object System.IO.MemoryStream; $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png); return ,$ms.ToArray() }

$round = Draw-Master 2048 $false
$square = Draw-Master 2048 $true

(Scale-To $round 96).Save((Join-Path $OutDir 'favicon-96x96.png'), [System.Drawing.Imaging.ImageFormat]::Png)
(Scale-To $square 180).Save((Join-Path $OutDir 'apple-touch-icon.png'), [System.Drawing.Imaging.ImageFormat]::Png)

# ICO: header, one directory entry per size, then the PNG payloads.
$sizes = 16, 32, 48
$pngs = @(); foreach ($p in $sizes) { $pngs += ,(Png-Bytes (Scale-To $round $p)) }
$fs = [System.IO.File]::Create((Join-Path $OutDir 'favicon.ico'))
$w = New-Object System.IO.BinaryWriter $fs
$w.Write([UInt16]0); $w.Write([UInt16]1); $w.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
  $w.Write([byte]$sizes[$i]); $w.Write([byte]$sizes[$i]); $w.Write([byte]0); $w.Write([byte]0)
  $w.Write([UInt16]1); $w.Write([UInt16]32); $w.Write([UInt32]$pngs[$i].Length); $w.Write([UInt32]$offset)
  $offset += $pngs[$i].Length
}
foreach ($b in $pngs) { $w.Write($b) }
$w.Close()
"done"
