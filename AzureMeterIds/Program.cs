using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

var propertyNameToGetValue = typeof(PriceResponse)
    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
    .ToDictionary<PropertyInfo, string, Func<PriceResponse, object>>(
        x => x.Name,
        x => y => PropertyCallAdapterProvider<PriceResponse>.GetInstance(x.Name).InvokeGet(y)) ;

var excludedFromKeys = new[]
{
    nameof(PriceResponse.ExtraProperties),
    nameof(PriceResponse.EffectiveEndDate),
    nameof(PriceResponse.EffectiveStartDate),
    nameof(PriceResponse.UnitPrice),
    nameof(PriceResponse.RetailPrice),
    nameof(PriceResponse.CurrencyCode),
};
var keyCombinations = Combinations(propertyNameToGetValue.Keys.Except(excludedFromKeys)).ToHashSet();
var hasDuplicates = new HashSet<string[]>(); // Reference equality is okay here since there is a fixed set of references.

var pricesFromFile = Directory
    .EnumerateFiles(@"C:\Users\jver\Desktop\page-prices", "page*.json")
    .Select(x =>
    {
        Console.WriteLine(x);
        return JsonSerializer.Deserialize<PricesResponse>(File.ReadAllText(x));
    })
    .SelectMany(x => x.Items)
    .ToList();

var propertyNamesToValueToPrices = keyCombinations.ToDictionary(x => x, x => new Dictionary<DynamicTuple, List<PriceResponse>>());

for (int i = 0; i < pricesFromFile.Count; i++)
{
    if (i % 1000 == 0)
    {
        Console.WriteLine("Price " + i);
    }

    PriceResponse? price = pricesFromFile[i];
    var duplicateFound = false;

    foreach (var keyCombination in keyCombinations)
    {
        var values = new object[keyCombination.Length];
        for (var j = 0; j < values.Length; j++)
        {
            values[j] = propertyNameToGetValue[keyCombination[j]](price);
        }

        var tuple = new DynamicTuple(values);
        var valueToPrices = propertyNamesToValueToPrices[keyCombination];

        if (!valueToPrices.TryGetValue(tuple, out var duplicates))
        {
            duplicates = new List<PriceResponse> { price };
            valueToPrices.Add(tuple, duplicates);
        }
        else
        {
            // Console.WriteLine("Duplicates: " + string.Join(" + ", keyCombination));
            duplicateFound = true;
            duplicates.Add(price);
            hasDuplicates.Add(keyCombination);
        }
    }

    if (duplicateFound)
    {
        keyCombinations.ExceptWith(hasDuplicates);
    }
}

Debugger.Launch();

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

static IEnumerable<T[]> Combinations<T>(IEnumerable<T> source)
{
    if (null == source)
        throw new ArgumentNullException(nameof(source));

    T[] data = source.ToArray();

    return Enumerable
      .Range(0, 1 << (data.Length))
      .Select(index => data
         .Where((v, i) => (index & (1 << i)) != 0)
         .ToArray());
}

enum ComputePriority
{
    Normal,
    LowPriority,
    Spot,
}

public interface IPropertyCallAdapter<TThis>
{
    object InvokeGet(TThis @this);
}

public class PropertyCallAdapter<TThis, TResult> : IPropertyCallAdapter<TThis>
{
    private readonly Func<TThis, TResult> _getterInvocation;

    public PropertyCallAdapter(Func<TThis, TResult> getterInvocation)
    {
        _getterInvocation = getterInvocation;
    }

    public object InvokeGet(TThis @this)
    {
        return _getterInvocation.Invoke(@this);
    }
}

public class PropertyCallAdapterProvider<TThis>
{
    private static readonly Dictionary<string, IPropertyCallAdapter<TThis>> _instances =
        new Dictionary<string, IPropertyCallAdapter<TThis>>();

    public static IPropertyCallAdapter<TThis> GetInstance(string forPropertyName)
    {
        IPropertyCallAdapter<TThis> instance;
        if (!_instances.TryGetValue(forPropertyName, out instance))
        {
            var property = typeof(TThis).GetProperty(
                forPropertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            MethodInfo getMethod;
            Delegate getterInvocation = null;
            if (property != null && (getMethod = property.GetGetMethod(true)) != null)
            {
                var openGetterType = typeof(Func<,>);
                var concreteGetterType = openGetterType
                    .MakeGenericType(typeof(TThis), property.PropertyType);

                getterInvocation =
                    Delegate.CreateDelegate(concreteGetterType, null, getMethod);
            }
            else
            {
                //throw exception or create a default getterInvocation returning null
            }

            var openAdapterType = typeof(PropertyCallAdapter<,>);
            var concreteAdapterType = openAdapterType
                .MakeGenericType(typeof(TThis), property.PropertyType);
            instance = Activator
                .CreateInstance(concreteAdapterType, getterInvocation)
                    as IPropertyCallAdapter<TThis>;

            _instances.Add(forPropertyName, instance);
        }

        return instance;
    }
}

public class DynamicTuple : IEquatable<DynamicTuple>
{
    public DynamicTuple(object[] values)
    {
        Values = values;

        var hashCode = new HashCode();
        foreach (var value in values)
        {
            hashCode.Add(value);
        }

        _hashCode = hashCode.ToHashCode();
    }

    public IReadOnlyList<object> Values { get; }

    private readonly int _hashCode;

    public override bool Equals(object? obj)
    {
        return obj is DynamicTuple tuple && Equals(tuple);
    }

    public bool Equals(DynamicTuple? other)
{
        return other is not null && Values.SequenceEqual(other.Values);
    }

    public override int GetHashCode() => _hashCode;
}

record Meter(string ArmRegionName, string ArmSkuName, ComputePriority ComputePriority, bool IsWindows, string MeterId);

public class ArmRegion
{
    public ArmRegion(string value)
    {
        Value = value;
    }

    public int ArmRegionId { get; set; }
    public string Value { get; set; }
}

public class ArmSku
{
    public ArmSku(string value)
    {
        Value = value;
    }

    public int ArmSkuId { get; set; }
    public string Value { get; set; }
}


public class PricesContext : DbContext
{
    public DbSet<ArmRegion> ArmRegions => Set<ArmRegion>();
    public DbSet<ArmSku> ArmSkus => Set<ArmSku>();

    public string DbPath { get; }

    public PricesContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "azure-prices.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");
}
