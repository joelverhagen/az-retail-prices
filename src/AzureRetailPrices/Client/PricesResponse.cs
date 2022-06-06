using System.Text.Json;
using System.Text.Json.Serialization;

namespace Knapcode.AzureRetailPrices.Client;

public class PricesResponse
{
    [JsonConstructor]
    public PricesResponse(
        string billingCurrency,
        string customerEntityId,
        string customerEntityType,
        PriceResponse[] items,
        string? nextPageLink,
        int count)
    {
        BillingCurrency = billingCurrency ?? throw new ArgumentNullException(nameof(billingCurrency));
        CustomerEntityId = customerEntityId ?? throw new ArgumentNullException(nameof(customerEntityId));
        CustomerEntityType = customerEntityType ?? throw new ArgumentNullException(nameof(customerEntityType));
        Items = items ?? throw new ArgumentNullException(nameof(items));
        NextPageLink = nextPageLink;
        Count = count;
    }

    public string BillingCurrency { get; set; }
    public string CustomerEntityId { get; set; }
    public string CustomerEntityType { get; set; }
    public PriceResponse[] Items { get; set; }
    public string? NextPageLink { get; set; }
    public int Count { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraProperties { get; set; }
}
