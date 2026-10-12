// Case 06 — review the public API of this type.

namespace Contoso.Throttling;

public sealed record TokenBucketOptions
{
    public int Capacity { get; init; } = 100;
    public TimeSpan RefillInterval { get; init; } = TimeSpan.FromSeconds(1);
    public int TokensPerRefill { get; init; } = 100;
}

/// <summary>
/// A token bucket that callers draw from. Safe for concurrent use. Disposing the bucket faults every
/// pending <see cref="AcquireAsync"/> with <see cref="ObjectDisposedException"/>; cancelling the token
/// passed to <see cref="AcquireAsync"/> faults that call with <see cref="OperationCanceledException"/>.
/// Refills never exceed <see cref="Capacity"/>. Pending <see cref="AcquireAsync"/> calls are served in
/// arrival order before newly refilled tokens are offered to <see cref="TryAcquire"/>.
/// </summary>
public sealed class TokenBucket : IDisposable
{
    private readonly Timer _refill;
    private int _tokens;
    private bool _disposed;

    public TokenBucket() : this(new TokenBucketOptions()) { }

    public TokenBucket(TokenBucketOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.Capacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.TokensPerRefill);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.RefillInterval, TimeSpan.Zero);
        Capacity = options.Capacity;
        _tokens = options.Capacity;
        _refill = new Timer(_ => Refill(options.TokensPerRefill), null, options.RefillInterval, options.RefillInterval);
    }

    public int Capacity { get; }

    /// <summary>Takes <paramref name="tokens"/> now if available; never waits.</summary>
    public bool TryAcquire(int tokens = 1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokens);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tokens, Capacity);
        return TryTake(tokens);
    }

    /// <summary>Takes <paramref name="tokens"/>, waiting for refills if needed.</summary>
    public ValueTask AcquireAsync(int tokens = 1, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokens);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tokens, Capacity);
        return TryTake(tokens) ? ValueTask.CompletedTask : WaitAsync(tokens, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _refill.Dispose();
        FaultWaiters();
    }

    private bool TryTake(int tokens) => throw null!;
    private ValueTask WaitAsync(int tokens, CancellationToken cancellationToken) => throw null!;
    private void Refill(int tokens) => throw null!;
    private void FaultWaiters() => throw null!;
}
