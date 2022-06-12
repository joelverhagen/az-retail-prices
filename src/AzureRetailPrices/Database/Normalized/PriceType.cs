namespace Knapcode.AzureRetailPrices.Database.Normalized;

public record PriceType
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}
