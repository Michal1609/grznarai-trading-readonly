using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.RateLimiting;

namespace GrznarAi.Trading.ReadOnly.Tests.RateLimiting;

[TestFixture]
public class SlidingWindowTests
{
    [Test]
    public async Task WaitAsync_AllowsConfiguredPermitLimitImmediately()
    {
        using var window = new SlidingWindow(2, TimeSpan.FromSeconds(10), TimeProvider.System);

        var stopwatch = Stopwatch.StartNew();
        await window.WaitAsync(CancellationToken.None);
        await window.WaitAsync(CancellationToken.None);
        stopwatch.Stop();

        Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromMilliseconds(500)));
    }

    [Test]
    [Category("Timing")]
    public async Task WaitAsync_WaitsAfterPermitWindowIsExhausted()
    {
        using var window = new SlidingWindow(1, TimeSpan.FromMilliseconds(75), TimeProvider.System);

        await window.WaitAsync(CancellationToken.None);
        var stopwatch = Stopwatch.StartNew();
        await window.WaitAsync(CancellationToken.None);
        stopwatch.Stop();

        Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(50)));
    }

    [Test]
    public async Task WaitAsync_CancellationWhileQueued_ThrowsOperationCanceledException()
    {
        using var window = new SlidingWindow(1, TimeSpan.FromSeconds(10), TimeProvider.System);
        await window.WaitAsync(CancellationToken.None);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));

        Assert.That(
            async () => await window.WaitAsync(cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }
}
