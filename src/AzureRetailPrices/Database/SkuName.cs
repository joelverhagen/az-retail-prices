namespace Knapcode.AzureRetailPrices.Database;

public record SkuName
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}
