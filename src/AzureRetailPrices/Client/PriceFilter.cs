namespace Knapcode.AzureRetailPrices;

public class PriceFilter : IPriceFilter
{
    public string ArmRegionName { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string MeterId { get; set; } = null!;
    public string MeterName { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string SkuId { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string SkuName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string ServiceId { get; set; } = null!;
    public string ServiceFamily { get; set; } = null!;
    public string PriceType { get; set; } = null!;
    public string ArmSkuName { get; set; } = null!;
}