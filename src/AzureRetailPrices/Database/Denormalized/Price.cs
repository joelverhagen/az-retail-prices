namespace Knapcode.AzureRetailPrices.Database.Denormalized;

public class Price
{
    public int Id { get; set; }

    public decimal TierMinimumUnits { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public string ArmRegionName { get; set; } = null!;
    public string Location { get; set; } = null!;
    public long EffectiveStartDate { get; set; }
    public string MeterId { get; set; } = null!;
    public string MeterName { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string SkuId { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string SkuName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string ServiceId { get; set; } = null!;
    public string ServiceFamily { get; set; } = null!;
    public string UnitOfMeasure { get; set; } = null!;
    public string PriceType { get; set; } = null!;
    public bool IsPrimaryMeterRegion { get; set; }
    public string ArmSkuName { get; set; } = null!;
    public string? ReservationTerm { get; set; }
    public long? EffectiveEndDate { get; set; }
}
