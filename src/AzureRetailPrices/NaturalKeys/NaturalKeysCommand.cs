using System.Collections.Concurrent;
using System.Diagnostics;
using Knapcode.AzureRetailPrices.Client;
using Knapcode.AzureRetailPrices.Reflection;

namespace Knapcode.AzureRetailPrices.NaturalKeys;

public static class NaturalKeysCommand
{
    public static async Task RunAsync()
    {
        var prices = DirectoryHelper.GetPricesFromLatestSnapshot();

        var noDuplicates = await GetNaturalKeyCandidates(prices.ToList());

        var naturalKeys = await FindNaturalKeys(noDuplicates);

        foreach (var keys in naturalKeys.OrderBy(x => x.Count))
        {
            Console.WriteLine("Natural key: " + string.Join(" + ", keys.OrderBy(x => x)));
        }
    }

    private static async Task<ConcurrentBag<HashSet<string>>> GetNaturalKeyCandidates(List<PriceResponse> pricesFromFile)
    {
        var noDuplicates = new ConcurrentBag<HashSet<string>>();
        var propertyNameToGetValue = ReflectionHelper.GetPropertyNameToGetScalarValue<PriceResponse>();

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
