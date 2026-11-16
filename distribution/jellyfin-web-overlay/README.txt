User filter menu: jellyfin-web overlay (12.4)
=============================================

License: GNU General Public License version 2 only (GPL-2.0-only), the same license as Jellyfin.
These files are modified jellyfin-web 12.4 source. The full text is ../../LICENSE.

Adds a CorrMedia entry for any signed-in user:
- User menu (AppUserMenu) → CorrMedia
- Settings → CorrMedia (own account only)
- /corrmedia?name=CorrMedia, a user route (not the admin dashboard) that loads the plugin filter page

Listing cards and lists show an Edits badge when the item JSON has HasCorrSidecar
(a corr.json sidecar is available, or a series/season has an episode with one).

Apply:
  powershell -File scripts/apply-web-overlay.ps1 -JellyfinWebPath C:\path\to\jellyfin-web

Then npm run build:production and serve dist with Jellyfin --webdir.
See distribution/APPLY_GUIDE.md.
