namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Known <c>status</c> values for a CFM futures sweep.
/// <list type="bullet">
///   <item><see cref="Pending"/> — sweep is cancellable.</item>
///   <item><see cref="Processing"/> — sweep has started and cannot be cancelled.</item>
/// </list>
/// </summary>
public static class FuturesSweepStatus
{
    public const string Unknown = "UNKNOWN_FCM_SWEEP_STATUS";
    public const string Pending = "PENDING";
    public const string Processing = "PROCESSING";
}
