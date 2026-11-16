using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Episode = MediaBrowser.Controller.Entities.TV.Episode;

namespace Jellyfin.Plugin.CorrMedia.Services;

/// <summary>
/// Tracks which library items have a corr.json sidecar so listings can badge them.
/// Videos are checked from their file path. Series and seasons are marked when any episode has one.
/// </summary>
public sealed class CorrSidecarIndex :
    IHostedService,
#if PATCHED_CORE
    ICorrSidecarAvailability,
#endif
    IDisposable
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<CorrSidecarIndex> _logger;
    private readonly object _gate = new();
    private readonly CancellationTokenSource _shutdown = new();
    private HashSet<Guid> _ids = new();
    private int _rebuilding;
    private int _lastSidecarCount = -1;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrSidecarIndex"/> class.
    /// </summary>
    /// <param name="libraryManager">Library manager.</param>
    /// <param name="logger">Logger.</param>
    public CorrSidecarIndex(ILibraryManager libraryManager, ILogger<CorrSidecarIndex> logger)
    {
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Reports whether this item has a corr.json sidecar.
    /// Videos are checked from their file. Series and seasons are true when any episode has one.
    /// </summary>
    /// <param name="item">Library item.</param>
    /// <returns>True when a sidecar is available.</returns>
    public bool HasSidecar(BaseItem item)
    {
        if (item is null || item.Id == Guid.Empty)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(item.Path) && item is Video)
        {
            var exists = CorrSidecarMarkers.MediaFileHasSidecar(item.Path);
            if (exists && item is Episode episode)
            {
                lock (_gate)
                {
                    CorrSidecarMarkers.Add(_ids, episode.Id, episode.SeriesId, episode.SeasonId);
                }
            }

            return exists;
        }

        lock (_gate)
        {
            return _ids.Contains(item.Id);
        }
    }

    /// <summary>
    /// Gets the item ids currently known to have a sidecar, including parent series and seasons.
    /// </summary>
    /// <returns>Item ids.</returns>
    public Guid[] Snapshot()
    {
        lock (_gate)
        {
            var copy = new Guid[_ids.Count];
            _ids.CopyTo(copy);
            return copy;
        }
    }

    /// <summary>
    /// Rebuilds the series and season index from library video files.
    /// </summary>
    internal void Rebuild()
    {
        if (Interlocked.Exchange(ref _rebuilding, 1) == 1)
        {
            return;
        }

        try
        {
            var items = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes =
                [
                    BaseItemKind.Movie,
                    BaseItemKind.Episode,
                    BaseItemKind.Video,
                    BaseItemKind.MusicVideo,
                    BaseItemKind.Trailer
                ],
                IsVirtualItem = false,
                Recursive = true
            });

            var ids = new HashSet<Guid>();
            var sidecarCount = 0;
            foreach (var item in items)
            {
                if (!CorrSidecarMarkers.MediaFileHasSidecar(item.Path))
                {
                    continue;
                }

                sidecarCount++;
                var seriesId = Guid.Empty;
                var seasonId = Guid.Empty;
                if (item is Episode episode)
                {
                    seriesId = episode.SeriesId;
                    seasonId = episode.SeasonId;
                }

                CorrSidecarMarkers.Add(ids, item.Id, seriesId, seasonId);
            }

            lock (_gate)
            {
                _ids = ids;
                if (sidecarCount != _lastSidecarCount)
                {
                    _lastSidecarCount = sidecarCount;
                    _logger.LogInformation("CorrMedia sidecar index updated: {Count} edit files", sidecarCount);
                }
            }
        }
        finally
        {
            Interlocked.Exchange(ref _rebuilding, 0);
        }
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => LoopAsync(_shutdown.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _shutdown.CancelAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _shutdown.Cancel();
        _shutdown.Dispose();
        _disposed = true;
    }

    private async Task LoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
            for (var pass = 0; !cancellationToken.IsCancellationRequested; pass++)
            {
                try
                {
                    Rebuild();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "CorrMedia sidecar index refresh failed");
                }

                var delay = pass < 4 ? TimeSpan.FromSeconds(20) : TimeSpan.FromMinutes(5);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Host is stopping.
        }
    }
}
