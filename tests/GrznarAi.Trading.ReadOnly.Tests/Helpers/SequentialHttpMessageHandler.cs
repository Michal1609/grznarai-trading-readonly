namespace GrznarAi.Trading.ReadOnly.Tests.Helpers;

internal sealed class SequentialHttpMessageHandler : HttpMessageHandler
{
    private readonly IReadOnlyList<Func<HttpRequestMessage, HttpResponseMessage>> _factories;
    private readonly List<HttpRequestMessage> _requests = [];
    private readonly object _sync = new();
    private int _callCount;

    public SequentialHttpMessageHandler(params Func<HttpResponseMessage>[] factories)
        : this(factories.Select<Func<HttpResponseMessage>, Func<HttpRequestMessage, HttpResponseMessage>>(
            factory => _ => factory()).ToArray())
    {
    }

    public SequentialHttpMessageHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] factories)
    {
        if (factories.Length == 0)
            throw new ArgumentException("At least one response factory is required.", nameof(factories));

        _factories = factories;
    }

    public int CallCount => Volatile.Read(ref _callCount);

    public IReadOnlyList<HttpRequestMessage> Requests
    {
        get
        {
            lock (_sync)
            {
                return _requests.ToArray();
            }
        }
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var index = Interlocked.Increment(ref _callCount) - 1;
        lock (_sync)
        {
            _requests.Add(request);
        }

        var factory = _factories[Math.Min(index, _factories.Count - 1)];
        return Task.FromResult(factory(request));
    }
}

