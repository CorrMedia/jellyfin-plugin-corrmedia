#nullable enable

using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Library;

/// <summary>
/// Lets plugins tell listings which items have a corr.json sidecar.
/// </summary>
public interface ICorrSidecarAvailability
{
    /// <summary>
    /// Returns true when a corr.json sidecar is available for this item.
    /// Series and seasons are included when any episode has a sidecar.
    /// </summary>
    /// <param name="item">Library item being sent to a client.</param>
    /// <returns>True when an edit file is available.</returns>
    bool HasSidecar(BaseItem item);
}
