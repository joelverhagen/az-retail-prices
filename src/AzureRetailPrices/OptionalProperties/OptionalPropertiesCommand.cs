using System.Text.Json;

namespace Knapcode.AzureRetailPrices.OptionalProperties;

public static class OptionalPropertiesCommand
{
    public static void Run()
    {
        var jsonItems = DirectoryHelper
            .GetLatestSnapshotFiles()
            .Select(x =>
            {
                Console.WriteLine("Reading: " + x);
                return JsonDocument.Parse(File.ReadAllText(x));
            })
            .SelectMany(x => x!.RootElement.GetProperty("Items").EnumerateArray())
            .ToList();

        var propertyNameToKindToCount = new Dictionary<string, Dictionary<JsonValueKind, int>>();
        var itemCount = 0;
        foreach (var item in jsonItems)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("The item must be a JSON object.");
            }

            itemCount++;

            foreach (var property in item.EnumerateObject())
            {
                if (!propertyNameToKindToCount.TryGetValue(property.Name, out var kindToCount))
                {
                    kindToCount = new Dictionary<JsonValueKind, int>();
                    propertyNameToKindToCount.Add(property.Name, kindToCount);
                }

                if (!kindToCount.TryGetValue(property.Value.ValueKind, out var count))
                {
                    kindToCount.Add(property.Value.ValueKind, 1);
                }
                else
                {
                    kindToCount[property.Value.ValueKind] = count + 1;
                }
            }
        }

        foreach (var propertyNamePair in propertyNameToKindToCount.OrderBy(x => x.Key))
        {
            var sum = propertyNamePair.Value.Sum(x => x.Value);
            Console.WriteLine($"Property: {propertyNamePair.Key} {(sum < itemCount ? "(optional)" : string.Empty)}");
            foreach (var kindPair in propertyNamePair.Value.OrderBy(x => x.Key))
            {
                Console.WriteLine($"  - {kindPair.Key}: {kindPair.Value}");
            }
        }
    }
}
