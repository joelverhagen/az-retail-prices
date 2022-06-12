namespace Knapcode.AzureRetailPrices.Database.Normalized;

public record Service
{
    public int Id { get; set; }

    public string ServiceId { get; set; } = null!;
    public string ServiceName { get; set; } = null!;

    public int ServiceFamilyId { get; set; }
    public ServiceFamily ServiceFamily { get; set; } = null!;
}
