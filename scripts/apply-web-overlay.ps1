#Requires -Version 5.1
<#
.SYNOPSIS
    Applies the jellyfin-web overlay into a local jellyfin-web checkout.
.DESCRIPTION
    Copies patched files from distribution/jellyfin-web-overlay into a
    jellyfin-web tree on release-12.z (12.4). Idempotent; prints what was copied.
    Rebuild the web client after applying (npm run build:production) and
    serve that dist with Jellyfin --webdir.
.PARAMETER JellyfinWebPath
    Path to the root of a jellyfin-web checkout (where package.json lives).
.PARAMETER OverlayPath
    Path to the overlay folder. Defaults to repo distribution/jellyfin-web-overlay.
.EXAMPLE
    .\apply-web-overlay.ps1 -JellyfinWebPath C:\jellyfin-web
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $JellyfinWebPath,

    [string] $OverlayPath
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir '..')).Path
if (-not $OverlayPath) {
    $OverlayPath = Join-Path $repoRoot 'distribution\jellyfin-web-overlay'
}

if (-not (Test-Path $OverlayPath)) {
    Write-Error "Overlay path not found: $OverlayPath"
}

if (-not (Test-Path $JellyfinWebPath)) {
    Write-Error "jellyfin-web path not found: $JellyfinWebPath"
}

$items = Get-ChildItem -Path $OverlayPath -Recurse -File
foreach ($f in $items) {
    $relative = $f.FullName.Substring($OverlayPath.Length).TrimStart('\', '/')
    $dest = Join-Path $JellyfinWebPath $relative
    $destDir = Split-Path -Parent $dest
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Force -Path $destDir | Out-Null
    }
    Copy-Item -Force -LiteralPath $f.FullName -Destination $dest
    Write-Host "Copied: $relative -> $dest"
}

Write-Host "Web overlay applied to $JellyfinWebPath"
Write-Host "Rebuild with: npm run build:production"
Write-Host "Serve that dist with Jellyfin --webdir"
