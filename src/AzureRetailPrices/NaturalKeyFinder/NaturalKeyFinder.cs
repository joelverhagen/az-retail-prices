using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Knapcode.AzureRetailPrices;

public static class NaturalKeyFinder
{
    public static async Task RunAsync()
    {
        var prices = GetPricesFromLatestSnapshot();

        var noDuplicates = await GetNaturalKeyCandidates(prices);

        var naturalKeys = await FindNaturalKeys(noDuplicates);

        foreach (var keys in naturalKeys.OrderBy(x => x.Count))
        {
            Console.WriteLine("Natural key: " + string.Join(" + ", keys.OrderBy(x => x)));
        }
    }

    private static List<PriceResponse> GetPricesFromLatestSnapshot()
    {
        var root = GetRoot();

        var latestSnapshot = Directory.EnumerateDirectories(Path.Combine(root, "snapshot")).OrderByDescending(x => x).Last();

        var pricesFromFile = Directory
            .EnumerateFiles(latestSnapshot, "page*.json")
            .Select(x =>
            {
                Console.WriteLine("Reading: " + x);
                return JsonSerializer.Deserialize<PricesResponse>(File.ReadAllText(x));
            })
            .SelectMany(x => x!.Items)
            .ToList();
        return pricesFromFile;
    }

    private static string GetRoot()
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

    private static async Task<ConcurrentBag<HashSet<string>>> GetNaturalKeyCandidates(List<PriceResponse> pricesFromFile)
    {
        var noDuplicates = new ConcurrentBag<HashSet<string>>();

        var propertyNameToGetValue = typeof(PriceResponse)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            .ToDictionary<PropertyInfo, string, Func<PriceResponse, object?>>(
                x => x.Name,
                x => y => PropertyCallAdapterProvider<PriceResponse>.GetInstance(x.Name).InvokeGet(y));

        var excludedFromKeys = new[]
        {
            nameof(PriceResponse.ExtraProperties),
            nameof(PriceResponse.EffectiveEndDate),
            nameof(PriceResponse.EffectiveStartDate),
            nameof(PriceResponse.UnitPrice),
            nameof(PriceResponse.RetailPrice),
            nameof(PriceResponse.CurrencyCode),
        };

        var allKeys = propertyNameToGetValue.Keys.Except(excludedFromKeys)
            .Combinations()
            .Select(x => new HashSet<string>(x))
            .OrderBy(x => x.Count)
            .ToList();

        var duplicateExamples = new ConcurrentDictionary<HashSet<string>, Dictionary<DynamicTuple, List<PriceResponse>>>();
        int keysCompleted = 0;
        var task = Task.Run(() =>
        {
            Parallel.ForEach(
                allKeys,
                keys =>
                {
                    foreach (var noDuplicate in noDuplicates)
                    {
                        if (keys.IsProperSupersetOf(noDuplicate))
                        {
                            noDuplicates.Add(keys);
                            return;
                        }
                    }

                    var valueToPrices = new Dictionary<DynamicTuple, List<PriceResponse>>();
                    var hasDuplicates = false;
                    var keyArray = keys.OrderBy(x => x).ToArray();

                    foreach (var price in pricesFromFile)
                    {
                        var values = new object?[keys.Count];
                        for (var j = 0; j < values.Length; j++)
                        {
                            values[j] = propertyNameToGetValue[keyArray[j]](price);
                        }

                        var tuple = new DynamicTuple(values);

                        if (!valueToPrices.TryGetValue(tuple, out var prices))
                        {
                            prices = new List<PriceResponse> { price };
                            valueToPrices.Add(tuple, prices);
                        }
                        else
                        {
                            prices.Add(price);
                            hasDuplicates = true;
                            break;
                        }
                    }

                    if (hasDuplicates)
                    {
                        duplicateExamples.TryAdd(
                            keys,
                            valueToPrices
                                .OrderByDescending(x => x.Value.Count)
                                .Take(5)
                                .ToDictionary(x => x.Key, x => x.Value));
                    }
                    else
                    {
                        noDuplicates.Add(keys);
                    }

                    Interlocked.Increment(ref keysCompleted);
                });
        });

        var sw = Stopwatch.StartNew();
        while (!task.IsCompleted)
        {
            Console.WriteLine($"[{sw.Elapsed}] Keys evaluated: {keysCompleted} / {allKeys.Count}");
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        await task;

        Console.WriteLine($"[{sw.Elapsed}] Complete.");
        return noDuplicates;
    }

    private static async Task<HashSet<HashSet<string>>> FindNaturalKeys(ConcurrentBag<HashSet<string>> noDuplicates)
    {
        var noDuplicatesArray = noDuplicates.ToArray();
        var supersets = new ConcurrentBag<HashSet<string>>();
        var noDuplicatesCompleted = 0;
        var task = Task.Run(() =>
        {
            Parallel.ForEach(
                noDuplicates,
                a =>
                {
                    foreach (var b in noDuplicatesArray)
                    {
                        if (a.IsProperSupersetOf(b))
                        {
                            supersets.Add(a);
                        }
                    }

                    Interlocked.Increment(ref noDuplicatesCompleted);
                });
        });

        var sw = Stopwatch.StartNew();
        while (!task.IsCompleted)
        {
            Console.WriteLine($"[{sw.Elapsed}] Supersets checked: {noDuplicatesCompleted} / {noDuplicates.Count}");
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        await task;

        Console.WriteLine($"[{sw.Elapsed}] Complete.");

        var noDuplicatsSet = noDuplicates.ToHashSet();
        noDuplicatsSet.ExceptWith(supersets);

        return noDuplicatsSet;
    }
}
