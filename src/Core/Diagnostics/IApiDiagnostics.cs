namespace GrznarAi.Trading.ReadOnly.Diagnostics;

/// <summary>
/// Read-only view of captured HTTP diagnostic snapshots for the current client instance.
/// Implementations must be fully thread-safe — HTTP clients are often called from multiple
/// concurrent tasks within the same scope.
/// </summary>
public interface IApiDiagnostics
{
    /// <summary>The most recently captured snapshot. <see langword="null"/> before any request is made.</summary>
    ApiResponseSnapshot? Last { get; }

    /// <summary>
    /// The last <c>N</c> captured snapshots, newest first.
    /// Returns a stable copy — safe to enumerate while new requests are in flight.
    /// </summary>
    IReadOnlyList<ApiResponseSnapshot> History { get; }

    /// <summary>Clears captured history and resets <see cref="Last"/> to <see langword="null"/>.</summary>
    void Clear();
}
