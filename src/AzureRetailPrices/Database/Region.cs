namespace Knapcode.AzureRetailPrices.Database;

public class Region
{
    public int RegionId { get; set; }
    public string ArmRegionName { get; set; } = null!;
    public string Location { get; set; } = null!;
}
