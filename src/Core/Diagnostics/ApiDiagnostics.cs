using GrznarAi.Trading.ReadOnly.Configuration;

namespace GrznarAi.Trading.ReadOnly.Diagnostics;

/// <summary>
/// Default thread-safe implementation of <see cref="IApiDiagnostics"/>.
/// </summary>
/// <remarks>
/// Thread-safety contract:
/// <list type="bullet">
///   <item><see cref="Last"/> uses <c>volatile</c> semantics via <see cref="Volatile"/> — atomic reference read/write.</item>
///   <item><see cref="_history"/> mutations are protected by <see cref="_sync"/> (short critical section — never holds lock across I/O).</item>
///   <item><see cref="History"/> returns a snapshot array — callers can enumerate freely without locking.</item>
///   <item><see cref="Add"/> performs the body-read allocation OUTSIDE the lock; only the final enqueue is locked.</item>
/// </list>
/// Register as singleton in DI. <c>DiagnosticCapturingHandler</c> takes this type directly.
/// </remarks>
public sealed class ApiDiagnostics : IApiDiagnostics
{
    private readonly int _historySize;
    private readonly object _sync = new();
    private ApiResponseSnapshot? _last;
    private readonly Queue<ApiResponseSnapshot> _history;

    public ApiDiagnostics(DiagnosticOptions options)
    {
        _historySize = Math.Max(1, options.HistorySize);
        _history = new Queue<ApiResponseSnapshot>(_historySize + 1);
    }

    public ApiResponseSnapshot? Last => Volatile.Read(ref _last);

    public IReadOnlyList<ApiResponseSnapshot> History
    {
        get
        {
            lock (_sync)
            {
                return _history.Reverse().ToArray();
            }
        }
    }

    public void Add(ApiResponseSnapshot snapshot)
    {
        Volatile.Write(ref _last, snapshot);
        lock (_sync)
        {
            if (_history.Count >= _historySize)
                _history.Dequeue();
            _history.Enqueue(snapshot);
        }
    }

    public void Clear()
    {
        Volatile.Write(ref _last, null);
        lock (_sync)
        {
            _history.Clear();
        }
    }
}
