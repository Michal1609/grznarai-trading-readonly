namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;

/// <summary>
/// Known <c>volume_type</c> values used in fee tier volume breakdown.
/// </summary>
public static class VolumeType
{
    public const string Spot = "VOLUME_TYPE_SPOT";
    public const string UsDerivatives = "VOLUME_TYPE_US_DERIVATIVES";
    public const string IntxDerivatives = "VOLUME_TYPE_INTX_DERIVATIVES";
    public const string Total = "VOLUME_TYPE_TOTAL";
}
