namespace Knapcode.AzureRetailPrices.Database;

public record ArmSkuName
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}