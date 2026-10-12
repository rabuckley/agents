// Case 08 — review the public API of this library.
// (Prior art in the BCL is System.IO.FileSystemWatcher: Created/Changed/Deleted/Renamed
// events, EnableRaisingEvents, Filter, NotifyFilter, IncludeSubdirectories, InternalBufferSize,
// an Error event, and IDisposable.)

namespace Contoso.IO;

public sealed record WatchOptions
{
    public bool IncludeSubdirectories { get; init; }
    public string Filter { get; init; } = "*";
    /// <summary>Changes buffered while the consumer is behind; the stream faults past this.</summary>
    public int MaxPendingChanges { get; init; } = 4096;
}

public abstract record FileChange
{
    private protected FileChange(string path) => Path = path;
    public string Path { get; }
}
public sealed record FileCreated(string Path) : FileChange(Path);
public sealed record FileModified(string Path) : FileChange(Path);
public sealed record FileDeleted(string Path) : FileChange(Path);
public sealed record FileRenamed(string OldPath, string Path) : FileChange(Path);

public static class DirectoryWatcher
{
    /// <summary>
    /// Streams changes under <paramref name="directory"/> until <paramref name="cancellationToken"/>
    /// is cancelled. Changes are buffered until read; if the consumer falls more than
    /// <see cref="WatchOptions.MaxPendingChanges"/> behind, or the underlying OS watch fails, the stream
    /// ends with an <see cref="IOException"/>.
    /// </summary>
    public static IAsyncEnumerable<FileChange> WatchAsync(
        string directory,
        WatchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(directory);
        if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(directory);
        return WatchCore(directory, options ?? new WatchOptions(), cancellationToken);
    }

    private static IAsyncEnumerable<FileChange> WatchCore(string directory, WatchOptions options, CancellationToken cancellationToken) => throw null!;
}
