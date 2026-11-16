using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.CorrMedia.Services;

/// <summary>
/// Refreshes <see cref="CorrSidecarIndex"/> after a library scan.
/// </summary>
public sealed class CorrSidecarIndexRefreshTask : ILibraryPostScanTask
{
    private readonly CorrSidecarIndex _index;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrSidecarIndexRefreshTask"/> class.
    /// </summary>
    /// <param name="index">Sidecar index.</param>
    public CorrSidecarIndexRefreshTask(CorrSidecarIndex index)
    {
        _index = index ?? throw new ArgumentNullException(nameof(index));
    }

    /// <inheritdoc />
    public Task Run(IProgress<double> progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(progress);
        cancellationToken.ThrowIfCancellationRequested();
        _index.Rebuild();
        progress.Report(100);
        return Task.CompletedTask;
    }
}
