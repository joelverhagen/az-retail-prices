namespace Knapcode.AzureRetailPrices.ComputePrices;

record Meter(string ArmSkuName, bool IsWindows, ComputePriority ComputePriority, string ArmRegionName);