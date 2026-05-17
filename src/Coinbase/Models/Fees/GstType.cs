namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;

/// <summary>
/// Known <c>type</c> values for <see cref="GoodsAndServicesTax"/>.
/// </summary>
public static class GstType
{
    /// <summary>Tax is included in the displayed price.</summary>
    public const string Inclusive = "INCLUSIVE";

    /// <summary>Tax is added on top of the displayed price.</summary>
    public const string Exclusive = "EXCLUSIVE";
}
