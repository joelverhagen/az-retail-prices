namespace Knapcode.AzureRetailPrices.Database;

public record Sku
{
    public int Id { get; set; }

    public int SkuNameId { get; set; }
    public SkuName SkuName { get; set; } = null!;
    public int ArmSkuNameId { get; set; }
    public ArmSkuName ArmSkuName { get; set; } = null!;
    public int RegionId { get; set; }
    public Region Region { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string SkuIdSuffix { get; set; } = null!;
    public int? ReservationTermId { get; set; }
    public ReservationTerm? ReservationTerm { get; set; }
}
