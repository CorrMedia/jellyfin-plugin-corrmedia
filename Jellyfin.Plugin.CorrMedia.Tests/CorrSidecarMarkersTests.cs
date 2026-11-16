using Jellyfin.Plugin.CorrMedia;
using Jellyfin.Plugin.CorrMedia.Services;

namespace Jellyfin.Plugin.CorrMedia.Tests;

public sealed class CorrSidecarMarkersTests
{
    [Fact]
    public void MediaFileHasSidecar_FalseWhenMissing()
    {
        var media = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "clip.mkv");
        Assert.False(CorrSidecarMarkers.MediaFileHasSidecar(media));
        Assert.False(CorrSidecarMarkers.MediaFileHasSidecar(null));
        Assert.False(CorrSidecarMarkers.MediaFileHasSidecar("  "));
    }

    [Fact]
    public void MediaFileHasSidecar_TrueWhenCorrJsonExists()
    {
        var directory = Path.Combine(Path.GetTempPath(), "corrmedia-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var media = Path.Combine(directory, "clip.mkv");
            File.WriteAllText(media, "x");
            Assert.False(CorrSidecarMarkers.MediaFileHasSidecar(media));
            File.WriteAllText(CorrFile.GetPath(media), "{}");
            Assert.True(CorrSidecarMarkers.MediaFileHasSidecar(media));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Add_IncludesSeriesAndSeason()
    {
        var ids = new HashSet<Guid>();
        var item = Guid.NewGuid();
        var series = Guid.NewGuid();
        var season = Guid.NewGuid();
        CorrSidecarMarkers.Add(ids, item, series, season);
        Assert.Contains(item, ids);
        Assert.Contains(series, ids);
        Assert.Contains(season, ids);
    }

    [Fact]
    public void Add_SkipsEmptyParents()
    {
        var ids = new HashSet<Guid>();
        var item = Guid.NewGuid();
        CorrSidecarMarkers.Add(ids, item, Guid.Empty, Guid.Empty);
        Assert.Contains(item, ids);
        Assert.Single(ids);
    }
}
