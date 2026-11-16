using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Jellyfin.Plugin.CorrMedia.Services;

/// <summary>
/// Sidecar existence checks and the ids listings should mark.
/// </summary>
internal static class CorrSidecarMarkers
{
    /// <summary>
    /// Returns true when <paramref name="mediaPath"/> has a sibling corr.json file.
    /// </summary>
    /// <param name="mediaPath">Media file path.</param>
    /// <returns>True when the sidecar file exists.</returns>
    [SuppressMessage("Security", "CA3003:Review code for file path injection vulnerabilities", Justification = "Callers pass a library item file path.")]
    public static bool MediaFileHasSidecar(string? mediaPath)
    {
        if (string.IsNullOrWhiteSpace(mediaPath))
        {
            return false;
        }

        try
        {
            return File.Exists(CorrFile.GetPath(mediaPath));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Records a video and, when set, its season and series.
    /// </summary>
    /// <param name="ids">Ids that should show an edits badge.</param>
    /// <param name="itemId">Video item id.</param>
    /// <param name="seriesId">Series id, or empty.</param>
    /// <param name="seasonId">Season id, or empty.</param>
    public static void Add(HashSet<Guid> ids, Guid itemId, Guid seriesId, Guid seasonId)
    {
        ArgumentNullException.ThrowIfNull(ids);
        if (itemId != Guid.Empty)
        {
            ids.Add(itemId);
        }

        if (seriesId != Guid.Empty)
        {
            ids.Add(seriesId);
        }

        if (seasonId != Guid.Empty)
        {
            ids.Add(seasonId);
        }
    }
}
