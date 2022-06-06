namespace Knapcode.AzureRetailPrices;

public class ArmSku
{
    public ArmSku(string value)
    {
        Value = value;
    }

    public int ArmSkuId { get; set; }
    public string Value { get; set; }
}
