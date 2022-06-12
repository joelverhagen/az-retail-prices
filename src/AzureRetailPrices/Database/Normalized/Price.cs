namespace Knapcode.AzureRetailPrices.Database.Normalized;

public class Price
{
    public int Id { get; set; }

    public int MeterId { get; set; }
    public Meter Meter { get; set; } = null!;
    public int MeterNameId { get; set; }
    public MeterName MeterName { get; set; } = null!;
    public int PriceTypeId { get; set; }
    public PriceType PriceType { get; set; } = null!;
    public int SkuId { get; set; }
    public Sku Sku { get; set; } = null!;
    public decimal TierMinimumUnits { get; set; }

    public decimal RetailPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public long EffectiveStartDate { get; set; }
    public bool IsPrimaryMeterRegion { get; set; }
    public long? EffectiveEndDate { get; set; }
}
