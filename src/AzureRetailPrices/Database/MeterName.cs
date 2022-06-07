namespace Knapcode.AzureRetailPrices.Database;

public record MeterName
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}
