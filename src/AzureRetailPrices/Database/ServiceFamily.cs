namespace Knapcode.AzureRetailPrices.Database;

public record ServiceFamily
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}
