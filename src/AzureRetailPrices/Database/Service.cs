namespace Knapcode.AzureRetailPrices.Database;

public class Service
{
    public int ServiceId { get; set; }
    public string ServiceIdValue { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    
    public ServiceFamily ServiceFamily { get; set; } = null!;
}
