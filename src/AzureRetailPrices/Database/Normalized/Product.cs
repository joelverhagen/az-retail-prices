namespace Knapcode.AzureRetailPrices.Database.Normalized;

public record Product
{
    public int Id { get; set; }

    public string ProductId { get; set; } = null!;
    public string ProductName { get; set; } = null!;

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
}
