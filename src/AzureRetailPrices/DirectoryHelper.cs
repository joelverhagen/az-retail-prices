using System.Text.Json;
using Knapcode.AzureRetailPrices.Client;

namespace Knapcode.AzureRetailPrices;

public static class DirectoryHelper
{
    public static IEnumerable<PriceResponse> GetPricesFromLatestSnapshot()
    {
        return GetLatestSnapshotFiles()
            .Select(x =>
            {
                Console.WriteLine("Reading: " + x);
                return JsonSerializer.Deserialize<PricesResponse>(File.ReadAllText(x));
            })
            .SelectMany(x => x!.Items);
    }

    public static IEnumerable<string> GetLatestSnapshotFiles()
    {
        var root = GetRoot();

        var latestSnapshot = Directory.EnumerateDirectories(Path.Combine(root, "snapshot")).OrderByDescending(x => x).Last();
        var files = Directory.EnumerateFiles(latestSnapshot, "page*.json");
        return files;
    }

    public static string GetRoot()
    {
        var root = Directory.GetCurrentDirectory();
        while (root != null)
        {
            var markerFile = Directory.EnumerateFiles(root, "AzureRetailPrices.sln");
            if (markerFile.Any())
            {
                break;
            }

            root = Path.GetDirectoryName(root);
        }

        if (root == null)
        {
            throw new InvalidOperationException("Could not find the repository root.");
        }

        return root;
    }
}
