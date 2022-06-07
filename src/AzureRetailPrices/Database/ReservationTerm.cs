namespace Knapcode.AzureRetailPrices.Database;

public record ReservationTerm
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}
