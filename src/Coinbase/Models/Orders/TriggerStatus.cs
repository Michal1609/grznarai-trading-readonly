namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>trigger_status</c> values for stop orders returned by the Coinbase API.</summary>
public static class TriggerStatus
{
    public const string Unknown = "UNKNOWN_TRIGGER_STATUS";
    public const string InvalidOrderType = "INVALID_ORDER_TYPE";
    public const string StopPending = "STOP_PENDING";
    public const string StopTriggered = "STOP_TRIGGERED";
}
