using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Knapcode.AzureRetailPrices.Client;
using Knapcode.AzureRetailPrices.Reflection;

namespace Knapcode.AzureRetailPrices.PropertyRelationshipFinder
{
    public static class PropertyRelationshipFinderCommand
    {
        public static async Task RunAsync()
        {
            var prices = DirectoryHelper.GetPricesFromLatestSnapshot().ToList();
            var propertyNameToGetValue = ReflectionHelper.GetPropertyNameToGetStringValue<PriceResponse>();
            propertyNameToGetValue.Remove(nameof(PriceResponse.CurrencyCode));
            var propertyNames = propertyNameToGetValue.Keys.ToHashSet();

            var strings = new ConcurrentDictionary<string, string>();
            var instances = new ConcurrentDictionary<ValueNode, ValueNode>();
            var associations = new ConcurrentDictionary<ValueNode, ConcurrentDictionary<ValueNode, bool>>(ReferenceEqualityComparer.Instance);
            var completed = 0;
            var task = Task.Run(() =>
            {
                Parallel.ForEach(
                    prices,
                    price =>
                    {
                        var nodes = new List<ValueNode>(propertyNameToGetValue.Count);
                        foreach (var pair in propertyNameToGetValue)
                        {
                            var originalValue = pair.Value(price);
                            var value = originalValue != null ? strings.GetOrAdd(originalValue, x => x) : null;
                            var node = instances.GetOrAdd(new ValueNode(pair.Key, value), x => x);
                            nodes.Add(node);
                        }

                        for (var i = 0; i < nodes.Count; i++)
                        {
                            var fromSet = associations.GetOrAdd(nodes[i], _ => new ConcurrentDictionary<ValueNode, bool>(ReferenceEqualityComparer.Instance));
                            for (var j = 0; j < nodes.Count; j++)
                            {
                                if (i != j)
                                {
                                    fromSet.TryAdd(nodes[j], true);
                                }
                            }
                        }

                        Interlocked.Increment(ref completed);
                    });
            });

            var sw = Stopwatch.StartNew();
            while (!task.IsCompleted)
            {
                Console.WriteLine($"[{sw.Elapsed}] Prices evaluated: {completed} / {prices.Count}");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            
            await task;

            Console.WriteLine("Finding single object associations for any subject...");
            var counts = associations
                .GroupBy(
                    a => a.Key,
                    a => a
                        .Value
                        .GroupBy(b => b.Key.PropertyName)
                        .Select(b => new { PropertyName = b.Key, Values = b.Select(b => b.Key.Value).ToList() })
                        .Where(b => b.Values.Count == 1)
                        .Select(b => new { b.PropertyName, Value = b.Values[0] })
                        .ToDictionary(b => b.PropertyName, b => b.Value))
                .GroupBy(
                    a => a.Key.PropertyName,
                    a => new { a.Key.Value, Associations = a.Single() });

            Console.WriteLine("Finding single object associations for all subjects...");
            var allImplies = new HashSet<KeyValuePair<string, string>>();
            foreach (var aValues in counts.OrderBy(x => x.Key))
            {
                var bCandidates = new HashSet<string>(propertyNames);

                foreach (var a in aValues)
                {
                    bCandidates.IntersectWith(a.Associations.Keys);
                }

                if (bCandidates.Any())
                {
                    foreach (var b in bCandidates)
                    {
                        allImplies.Add(KeyValuePair.Create(aValues.Key, b));
                    }
                }
            }

            Console.WriteLine("Finding equivalencies between subjects and objects...");
            var equivalencies = new HashSet<Tuple<string, string>>();
            foreach (var pair in allImplies.ToList())
            {
                var (a, b) = pair;
                var reverse = KeyValuePair.Create(b, a);
                if (allImplies.Contains(reverse))
                {
                    var equivalency = new[] { a, b }.OrderBy(x => x).ToArray();
                    equivalencies.Add(Tuple.Create(equivalency[0], equivalency[1]));
                    allImplies.Remove(pair);
                    allImplies.Remove(reverse);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Implies:");
            foreach (var implies in allImplies)
            {
                Console.WriteLine($"  - {implies.Key} => {implies.Value}");
            }

            Console.WriteLine();
            Console.WriteLine("Equivalencies:");
            foreach (var equivalency in equivalencies)
            {
                Console.WriteLine($"  - {equivalency.Item1} = {equivalency.Item2}");
            }
        }
    }

    public record ValueNode(string PropertyName, string? Value);
}
