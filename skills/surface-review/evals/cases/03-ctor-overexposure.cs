// Case 03 — review the public API of this type.

namespace Contoso.Resilience;

public sealed class RetryPolicy
{
    public RetryPolicy(int maxAttempts)
        : this(maxAttempts, TimeSpan.FromMilliseconds(200)) { }

    public RetryPolicy(int maxAttempts, TimeSpan baseDelay)
        : this(maxAttempts, baseDelay, TimeSpan.FromSeconds(30)) { }

    public RetryPolicy(int maxAttempts, TimeSpan baseDelay, TimeSpan maxDelay)
        : this(maxAttempts, baseDelay, maxDelay, exponential: true, jitter: true) { }

    public RetryPolicy(int maxAttempts, TimeSpan baseDelay, TimeSpan maxDelay, bool exponential, bool jitter)
        : this(maxAttempts, baseDelay, maxDelay, exponential, jitter, retryOnTimeout: true, shouldRetry: null, logger: null) { }

    public RetryPolicy(
        int maxAttempts,
        TimeSpan baseDelay,
        TimeSpan maxDelay,
        bool exponential,
        bool jitter,
        bool retryOnTimeout,
        Func<Exception, bool>? shouldRetry,
        ILogger? logger)
    {
        MaxAttempts = maxAttempts;
        BaseDelay = baseDelay;
        MaxDelay = maxDelay;
        Exponential = exponential;
        Jitter = jitter;
        RetryOnTimeout = retryOnTimeout;
        ShouldRetry = shouldRetry;
        Logger = logger;
    }

    public int MaxAttempts { get; }
    public TimeSpan BaseDelay { get; }
    public TimeSpan MaxDelay { get; }
    public bool Exponential { get; }
    public bool Jitter { get; }
    public bool RetryOnTimeout { get; }
    public Func<Exception, bool>? ShouldRetry { get; }
    public ILogger? Logger { get; }

    public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        => RetryCore(action, cancellationToken);

    private Task<T> RetryCore<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken) => throw null!;
}
