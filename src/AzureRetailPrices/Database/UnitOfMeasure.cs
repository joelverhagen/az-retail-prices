namespace Knapcode.AzureRetailPrices.Database;

public record UnitOfMeasure
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}
