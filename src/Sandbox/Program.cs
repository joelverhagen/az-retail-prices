using System.Net.Http.Headers;
using System.Text.Json;
using Knapcode.AzureRetailPrices.Client;
using Knapcode.AzureRetailPrices.LoadDatabase;
using Knapcode.AzureRetailPrices.OptionalPropertyFinder;
using Knapcode.AzureRetailPrices.PropertyRelationshipFinder;

await PropertyRelationshipFinderCommand.RunAsync();
// LoadDatabaseCommand.Run();

return;

using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AzureMeterIds", "0.0.1"));

var client = new PricesClient(httpClient);
var prices = client.GetPricesAsync(query: q => q
    .Where(x => x.ServiceName == "Virtual Machines")
    .Where(x => x.ServiceFamily == "Compute")
    .Where(x => x.PriceType == "Consumption")
    .Where(x => x.ArmRegionName.Contains("westus"))
    // .Where(x => x.MeterName.EndsWith(" Spot"))
    .OrderBy(x => x.ArmSkuName)
    .ThenBy(x => x.ProductName)
    .ThenBy(x => x.MeterName)
    .ThenBy(x => x.ArmRegionName));

var meterToPrice = new Dictionary<Meter, List<PriceResponse>>();

await foreach (var price in prices)
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

    var meter = new Meter(price.ArmRegionName, price.ArmSkuName, computePriority, isWindows, price.MeterId);

    if (!meterToPrice.TryGetValue(meter, out var matches))
    {
        matches = new List<PriceResponse>();
        meterToPrice.Add(meter, matches);
    }

    matches.Add(price);
}

Console.WriteLine("Writing...");
File.WriteAllText("prices.json", JsonSerializer.Serialize(meterToPrice
    .Where(x => x.Value.Count > 1)
    .Select(x => new { Meter = x.Key, Prices = x.Value })));

enum ComputePriority
{
    Normal,
    LowPriority,
    Spot,
}

record Meter(string ArmRegionName, string ArmSkuName, ComputePriority ComputePriority, bool IsWindows, string MeterId);
