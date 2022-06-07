using System.Text.Json;
using System.Text.Json.Serialization;

namespace Knapcode.AzureRetailPrices.Client;

public class PriceResponse
{
    [JsonConstructor]
    public PriceResponse(
        string currencyCode,
        decimal? tierMinimumUnits,
        decimal? retailPrice,
        decimal? unitPrice,
        string armRegionName,
        string location,
        DateTimeOffset? effectiveStartDate,
        string meterId,
        string meterName,
        string productId,
        string skuId,
        string productName,
        string skuName,
        string serviceName,
        string serviceId,
        string serviceFamily,
        string unitOfMeasure,
        string priceType,
        bool? isPrimaryMeterRegion,
        string armSkuName,
        string? reservationTerm,
        DateTimeOffset? effectiveEndDate)
    {
        CurrencyCode = currencyCode ?? throw new ArgumentNullException(nameof(currencyCode));
        TierMinimumUnits = tierMinimumUnits ?? throw new ArgumentNullException(nameof(tierMinimumUnits));
        RetailPrice = retailPrice ?? throw new ArgumentNullException(nameof(retailPrice));
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        ArmRegionName = armRegionName ?? throw new ArgumentNullException(nameof(armRegionName));
        Location = location ?? throw new ArgumentNullException(nameof(location));
        EffectiveStartDate = effectiveStartDate ?? throw new ArgumentNullException(nameof(effectiveStartDate));
        MeterId = meterId ?? throw new ArgumentNullException(nameof(meterId));
        MeterName = meterName ?? throw new ArgumentNullException(nameof(meterName));
        ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
        SkuId = skuId ?? throw new ArgumentNullException(nameof(skuId));
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        SkuName = skuName ?? throw new ArgumentNullException(nameof(skuName));
        ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        ServiceId = serviceId ?? throw new ArgumentNullException(nameof(serviceId));
        ServiceFamily = serviceFamily ?? throw new ArgumentNullException(nameof(serviceFamily));
        UnitOfMeasure = unitOfMeasure ?? throw new ArgumentNullException(nameof(unitOfMeasure));
        PriceType = priceType ?? throw new ArgumentNullException(nameof(priceType));
        IsPrimaryMeterRegion = isPrimaryMeterRegion ?? throw new ArgumentNullException(nameof(isPrimaryMeterRegion));
        ArmSkuName = armSkuName ?? throw new ArgumentNullException(nameof(armSkuName));
        ReservationTerm = reservationTerm;
        EffectiveEndDate = effectiveEndDate;
    }

    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; }

    [JsonPropertyName("tierMinimumUnits")]
    public decimal? TierMinimumUnits { get; set; }

    [JsonPropertyName("retailPrice")]
    public decimal? RetailPrice { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal? UnitPrice { get; set; }

    [JsonPropertyName("armRegionName")]
    public string ArmRegionName { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }

    [JsonPropertyName("effectiveStartDate")]
    public DateTimeOffset? EffectiveStartDate { get; set; }

    [JsonPropertyName("meterId")]
    public string MeterId { get; set; }

    [JsonPropertyName("meterName")]
    public string MeterName { get; set; }

    [JsonPropertyName("productId")]
    public string ProductId { get; set; }

    [JsonPropertyName("skuId")]
    public string SkuId { get; set; }

    [JsonPropertyName("productName")]
    public string ProductName { get; set; }

    [JsonPropertyName("skuName")]
    public string SkuName { get; set; }

    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }

    [JsonPropertyName("serviceId")]
    public string ServiceId { get; set; }

    [JsonPropertyName("serviceFamily")]
    public string ServiceFamily { get; set; }

    [JsonPropertyName("unitOfMeasure")]
    public string UnitOfMeasure { get; set; }

    [JsonPropertyName("type")]
    public string PriceType { get; set; }

    [JsonPropertyName("isPrimaryMeterRegion")]
    public bool? IsPrimaryMeterRegion { get; set; }

    [JsonPropertyName("armSkuName")]
    public string ArmSkuName { get; set; }

    [JsonPropertyName("reservationTerm")]
    public string? ReservationTerm { get; set; }

    [JsonPropertyName("effectiveEndDate")]
    public DateTimeOffset? EffectiveEndDate { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraProperties { get; set; }
}
