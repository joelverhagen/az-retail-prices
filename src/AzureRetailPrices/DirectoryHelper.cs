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
        return Directory.EnumerateFiles(GetSnapshotDirectory(), "page*.json");
    }

    public static string GetSnapshotDirectory()
    {
        var root = GetRoot();
        return Path.Combine(root, "snapshot");
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
