namespace Knapcode.AzureRetailPrices;

public class ArmRegion
{
    public ArmRegion(string value)
    {
        Value = value;
    }

    public int ArmRegionId { get; set; }
    public string Value { get; set; }
}
