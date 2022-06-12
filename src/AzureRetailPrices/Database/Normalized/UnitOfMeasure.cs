namespace Knapcode.AzureRetailPrices.Database.Normalized;

public record UnitOfMeasure
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}
