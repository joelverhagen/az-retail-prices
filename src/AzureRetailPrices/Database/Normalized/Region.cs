namespace Knapcode.AzureRetailPrices.Database.Normalized;

public record Region
{
    public int Id { get; set; }

    public string ArmRegionName { get; set; } = null!;
    public string Location { get; set; } = null!;
}
