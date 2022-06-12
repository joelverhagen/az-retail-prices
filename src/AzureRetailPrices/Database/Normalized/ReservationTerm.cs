namespace Knapcode.AzureRetailPrices.Database.Normalized;

public record ReservationTerm
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;
}
