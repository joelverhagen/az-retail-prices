using Knapcode.AzureRetailPrices.Client;

namespace Knapcode.AzureRetailPrices.ComputePrices;

public static class ComputePricesCommand
{
    public static void Run()
    {
        var prices = DirectoryHelper
            .GetPricesFromLatestSnapshot()
            .Where(x => x.ServiceFamily == "Compute")
            .Where(x => x.ServiceName == "Virtual Machines")
            .Where(x => x.PriceType == "Consumption")
            .Where(x => x.ArmSkuName.Length > 0);

        var meterToPrice = new Dictionary<Meter, List<PriceResponse>>();

        foreach (var price in prices)
        {
            ComputePriority computePriority;

            if (price.MeterName.EndsWith(" Spot"))
            {
                computePriority = ComputePriority.Spot;
            }
            else if (price.MeterName.EndsWith(" Low Priority"))
            {
                computePriority = ComputePriority.LowPriority;
            }
            else
            {
                computePriority = ComputePriority.Normal;
            }

            var isWindows = price.ProductName.EndsWith(" Windows");

            var meter = new Meter(price.ArmSkuName, isWindows, computePriority, price.ArmRegionName);

            if (!meterToPrice.TryGetValue(meter, out var matches))
            {
                matches = new List<PriceResponse>();
                meterToPrice.Add(meter, matches);
            }

            matches.Add(price);
        }

        foreach (var pair in meterToPrice)
        {
            if (pair.Value.Count > 1)
            {
                throw new InvalidDataException($"There are multiple ({pair.Value.Count}x) prices for a compute meter: " + pair.Key);
            }
        }

        foreach (var pair in meterToPrice
            .OrderBy(x => x.Key.ArmSkuName)
            .ThenBy(x => x.Key.IsWindows)
            .ThenBy(x => x.Key.ComputePriority)
            .ThenBy(x => x.Key.ArmRegionName))
        {
            Console.WriteLine(pair.Key + " " + pair.Value.Single().RetailPrice);
        }
    }
}
