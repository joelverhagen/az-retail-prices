namespace Knapcode.AzureRetailPrices.Database.Normalized;

public record MeterName
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}
