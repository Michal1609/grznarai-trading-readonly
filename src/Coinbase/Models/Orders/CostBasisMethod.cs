namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>cost_basis_method</c> values returned by the Coinbase API.</summary>
public static class CostBasisMethod
{
    public const string Unspecified = "COST_BASIS_METHOD_UNSPECIFIED";
    public const string Hifo = "COST_BASIS_METHOD_HIFO";
    public const string Lifo = "COST_BASIS_METHOD_LIFO";
    public const string Fifo = "COST_BASIS_METHOD_FIFO";
}
