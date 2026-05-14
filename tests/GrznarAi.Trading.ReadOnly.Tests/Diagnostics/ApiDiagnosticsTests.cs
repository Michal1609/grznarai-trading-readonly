using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.Diagnostics;

namespace GrznarAi.Trading.ReadOnly.Tests.Diagnostics;

[TestFixture]
public class ApiDiagnosticsTests
{
    [Test]
    public void History_ReturnsNewestFirstAndTrimsOldestSnapshots()
    {
        var diagnostics = new ApiDiagnostics(new DiagnosticOptions { HistorySize = 2 });

        diagnostics.Add(new ApiResponseSnapshot { StatusCode = 200, ResponseBody = "first" });
        diagnostics.Add(new ApiResponseSnapshot { StatusCode = 201, ResponseBody = "second" });
        diagnostics.Add(new ApiResponseSnapshot { StatusCode = 202, ResponseBody = "third" });

        Assert.That(diagnostics.Last?.ResponseBody, Is.EqualTo("third"));
        Assert.That(diagnostics.History.Select(snapshot => snapshot.ResponseBody), Is.EqualTo(new[] { "third", "second" }));
    }

    [Test]
    public void Clear_RemovesHistoryAndLastSnapshot()
    {
        var diagnostics = new ApiDiagnostics(new DiagnosticOptions { HistorySize = 2 });
        diagnostics.Add(new ApiResponseSnapshot { StatusCode = 200 });

        diagnostics.Clear();

        Assert.That(diagnostics.Last, Is.Null);
        Assert.That(diagnostics.History, Is.Empty);
    }

    [Test]
    public void Add_IsThreadSafeUnderParallelWrites()
    {
        var diagnostics = new ApiDiagnostics(new DiagnosticOptions { HistorySize = 8 });

        Parallel.For(0, 10_000, i =>
        {
            diagnostics.Add(new ApiResponseSnapshot { StatusCode = i });
        });

        Assert.That(diagnostics.Last, Is.Not.Null);
        Assert.That(diagnostics.History, Has.Count.EqualTo(8));
        Assert.That(diagnostics.History, Has.All.Matches<ApiResponseSnapshot>(snapshot => snapshot.StatusCode >= 0));
    }
}
