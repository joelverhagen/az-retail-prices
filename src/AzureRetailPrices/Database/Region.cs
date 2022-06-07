namespace Knapcode.AzureRetailPrices.Database;

public record Region
{
    public int Id { get; set; }

    public string ArmRegionName { get; set; } = null!;
    public string Location { get; set; } = null!;
}
