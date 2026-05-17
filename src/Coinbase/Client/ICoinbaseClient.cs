namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Unified Coinbase Advanced Trade API client facade combining all domain-specific client interfaces.
/// </summary>
public interface ICoinbaseClient : ICoinbaseAccountsClient, ICoinbaseConvertClient, ICoinbaseDataApiClient, ICoinbaseFeesClient, ICoinbaseFuturesClient, ICoinbaseOrdersClient, ICoinbasePaymentMethodsClient, ICoinbasePerpetualClient, ICoinbasePortfoliosClient, ICoinbaseProductsClient, ICoinbasePublicClient
{
}
