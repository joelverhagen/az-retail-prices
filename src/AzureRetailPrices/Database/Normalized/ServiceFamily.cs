namespace Knapcode.AzureRetailPrices.Database.Normalized;

public record ServiceFamily
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}
