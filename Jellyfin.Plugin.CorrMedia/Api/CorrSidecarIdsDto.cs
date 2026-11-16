using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.CorrMedia.Api;

/// <summary>
/// Library items that have a corr.json sidecar.
/// </summary>
public sealed class CorrSidecarIdsDto
{
    /// <summary>
    /// Gets or sets the item ids, including series and seasons that contain an edited episode.
    /// </summary>
    public IReadOnlyList<Guid> Ids { get; set; } = [];
}
