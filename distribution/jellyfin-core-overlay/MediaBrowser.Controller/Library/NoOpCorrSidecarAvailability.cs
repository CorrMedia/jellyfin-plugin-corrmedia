#nullable enable

using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Library;

/// <summary>
/// No-op <see cref="ICorrSidecarAvailability"/>.
/// </summary>
public sealed class NoOpCorrSidecarAvailability : ICorrSidecarAvailability
{
    /// <inheritdoc />
    public bool HasSidecar(BaseItem item) => false;
}
