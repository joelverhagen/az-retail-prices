using System.Data.Common;
using AutoMapper;
using Knapcode.AzureRetailPrices.Client;
using Normalized = Knapcode.AzureRetailPrices.Database.Normalized;
using Denormalized = Knapcode.AzureRetailPrices.Database.Denormalized;
using Knapcode.AzureRetailPrices.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Knapcode.AzureRetailPrices.LoadDatabase;

public static class LoadDatabaseCommand
{
    public static void Run()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = config.CreateMapper();
        var priceResponses = DirectoryHelper.GetPricesFromLatestSnapshot().ToList();

        Console.WriteLine();
        Console.WriteLine("==== LOADING DENORMALIZED DB ====");
        LoadDenormalized(mapper, priceResponses);
        Console.WriteLine();

        Console.WriteLine("==== LOADING NORMALIZED DB ====");
        LoadNormalized(mapper, priceResponses);
        Console.WriteLine();
    }
    private static void LoadDenormalized(IMapper mapper, List<PriceResponse> priceResponses)
    {
        var ctx = new Denormalized.PricesContext();
        File.Delete(ctx.DbPath);
        ctx.Database.EnsureCreatedAsync();

        Console.WriteLine($"Mapping {nameof(Denormalized.Price)}...");
        var prices = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Denormalized.Price>(x))
            .ToList();

        BulkLoad(ctx, prices
            .OrderBy(x => x.MeterId)
            .ThenBy(x => x.MeterName)
            .ThenBy(x => x.PriceType)
            .ThenBy(x => x.SkuId)
            .ThenBy(x => x.TierMinimumUnits));

        Console.WriteLine("Adding Prices view...");
        ctx.Database.ExecuteSqlRaw(@"
CREATE VIEW PricesView
AS
SELECT
    'USD' AS CurrencyCode,
    TierMinimumUnits,
    RetailPrice,
    UnitPrice,
    ArmRegionName,
    Location,
    strftime('%Y-%m-%dT%H:%M:%SZ', datetime(EffectiveStartDate, 'unixepoch')) AS EffectiveStartDate,
    MeterId,
    MeterName,
    ProductId,
    SkuId,
    ProductName,
    SkuName,
    ServiceName,
    ServiceId,
    ServiceFamily,
    UnitOfMeasure,
    PriceType,
    IsPrimaryMeterRegion,
    ArmSkuName,
    ReservationTerm,
    strftime('%Y-%m-%dT%H:%M:%SZ', datetime(EffectiveEndDate, 'unixepoch')) AS EffectiveEndDate
FROM Prices p
");

        Vacuum(ctx);
    }

    private static void LoadNormalized(IMapper mapper, List<PriceResponse> priceResponses)
    {
        var ctx = new Normalized.PricesContext();
        File.Delete(ctx.DbPath);
        ctx.Database.EnsureCreatedAsync();

        Console.WriteLine($"Mapping {nameof(Normalized.ServiceFamily)}...");
        var serviceFamilies = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.ServiceFamily>(x))
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.Service)}...");
        var services = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.Service>(x))
            .Select(x =>
            {
                x.ServiceFamily = serviceFamilies[x.ServiceFamily];
                return x;
            })
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.Product)}...");
        var products = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.Product>(x))
            .Select(x =>
            {
                x.Service = services[x.Service];
                return x;
            })
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.ArmSkuName)}...");
        var armSkuNames = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.ArmSkuName>(x))
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.SkuName)}...");
        var skuNames = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.SkuName>(x))
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.Region)}...");
        var regions = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.Region>(x))
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.ReservationTerm)}...");
        var reservationTerms = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.ReservationTerm>(x))
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.Sku)}...");
        var skus = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.Sku>(x))
            .Select(x =>
            {
                x.ArmSkuName = armSkuNames[x.ArmSkuName];
                x.SkuName = skuNames[x.SkuName];
                x.Region = regions[x.Region];
                x.Product = products[x.Product];
                x.ReservationTerm = x.ReservationTerm?.Value == null ? null : reservationTerms[x.ReservationTerm];

                return x;
            })
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.PriceType)}...");
        var priceTypes = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.PriceType>(x))
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.UnitOfMeasure)}...");
        var unitOfMeasures = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.UnitOfMeasure>(x))
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.Meter)}...");
        var meters = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.Meter>(x))
            .Select(x =>
            {
                x.UnitOfMeasure = unitOfMeasures[x.UnitOfMeasure];

                return x;
            })
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.MeterName)}...");
        var meterNames = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.MeterName>(x))
            .Distinct()
            .ToDictionary(x => x);

        Console.WriteLine($"Mapping {nameof(Normalized.Price)}...");
        var prices = priceResponses
            .AsParallel()
            .Select(x => mapper.Map<Normalized.Price>(x))
            .Select(x =>
            {
                if (x.Sku.ReservationTerm is not null && x.Sku.ReservationTerm.Value is null)
                {
                    x.Sku.ReservationTerm = null;
                }

                x.Meter = meters[x.Meter];
                x.MeterName = meterNames[x.MeterName];
                x.PriceType = priceTypes[x.PriceType];
                x.Sku = skus[x.Sku];

                return x;
            })
            .ToList();

        BulkLoad(ctx, serviceFamilies
            .Values
            .OrderBy(x => x.Value));

        BulkLoad(ctx, services
            .Values
            .Select(x =>
            {
                x.ServiceFamilyId = x.ServiceFamily.Id;
                return x;
            })
            .OrderBy(x => x.ServiceId));

        BulkLoad(ctx, products
            .Values
            .Select(x =>
            {
                x.ServiceId = x.Service.Id;
                return x;
            })
            .OrderBy(x => x.ProductId));

        BulkLoad(ctx, armSkuNames
            .Values
            .OrderBy(x => x.Value));

        BulkLoad(ctx, skuNames
            .Values
            .OrderBy(x => x.Value));

        BulkLoad(ctx, regions
            .Values
            .OrderBy(x => x.ArmRegionName));

        BulkLoad(ctx, reservationTerms
            .Values
            .Where(x => x.Value is not null)
            .OrderBy(x => x.Value));

        BulkLoad(ctx, skus
            .Values
            .Select(x =>
            {
                x.ArmSkuNameId = x.ArmSkuName.Id;
                x.SkuNameId = x.SkuName.Id;
                x.RegionId = x.Region.Id;
                x.ProductId = x.Product.Id;
                x.ReservationTermId = x.ReservationTerm?.Id;

                return x;
            })
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.SkuIdSuffix));

        BulkLoad(ctx, priceTypes.Values.OrderBy(x => x.Value));

        BulkLoad(ctx, unitOfMeasures.Values.OrderBy(x => x.Value));

        BulkLoad(ctx, meters
            .Values
            .Select(x =>
            {
                x.UnitOfMeasureId = x.UnitOfMeasure.Id;

                return x;
            })
            .OrderBy(x => x.MeterId));

        BulkLoad(ctx, meterNames
            .Values
            .OrderBy(x => x.Value));

        BulkLoad(ctx, prices
            .Select(x =>
            {
                x.MeterId = x.Meter.Id;
                x.MeterNameId = x.MeterName.Id;
                x.PriceTypeId = x.PriceType.Id;
                x.SkuId = x.Sku.Id;

                return x;
            })
            .OrderBy(x => x.MeterId)
            .ThenBy(x => x.MeterNameId)
            .ThenBy(x => x.PriceTypeId)
            .ThenBy(x => x.SkuId)
            .ThenBy(x => x.TierMinimumUnits));

        Console.WriteLine("Adding Prices view...");
        ctx.Database.ExecuteSqlRaw(@"
CREATE VIEW PricesView
AS
SELECT
    'USD' AS CurrencyCode,
    p.TierMinimumUnits,
    p.RetailPrice,
    p.UnitPrice,
    r.ArmRegionName,
    r.Location,
    strftime('%Y-%m-%dT%H:%M:%SZ', datetime(p.EffectiveStartDate, 'unixepoch')) AS EffectiveStartDate,
    m.MeterId,
    mn.Value AS MeterName,
    pr.ProductId,
    pr.ProductId || '/' || s.SkuIdSuffix AS SkuId,
    pr.ProductName,
    sn.Value AS SkuName,
    sr.ServiceName,
    sr.ServiceId,
    sf.Value AS ServiceFamily,
    uom.Value AS UnitOfMeasure,
    pt.Value AS PriceType,
    p.IsPrimaryMeterRegion,
    asn.Value AS ArmSkuName,
    rt.Value AS ReservationTerm,
    strftime('%Y-%m-%dT%H:%M:%SZ', datetime(p.EffectiveEndDate, 'unixepoch')) AS EffectiveEndDate
FROM Prices p
INNER JOIN Meters m ON p.MeterId = m.Id
INNER JOIN UnitOfMeasures uom ON m.UnitOfMeasureId = uom.Id
INNER JOIN MeterNames mn ON p.MeterNameId = mn.Id
INNER JOIN PriceTypes pt ON p.PriceTypeId = pt.Id
INNER JOIN Skus s ON p.SkuId = s.Id
INNER JOIN SkuNames sn ON s.SkuNameId = sn.Id
INNER JOIN ArmSkuNames asn ON s.ArmSkuNameId = asn.Id
INNER JOIN Regions r ON s.RegionId = r.Id
INNER JOIN Products pr ON s.ProductId = pr.Id
INNER JOIN Services sr ON pr.ServiceId = sr.Id
INNER JOIN ServiceFamilies sf ON sr.ServiceFamilyId = sf.Id
LEFT OUTER JOIN ReservationTerms rt ON s.ReservationTermId = rt.Id
");

        Vacuum(ctx);
    }

    private static void Vacuum(DbContext ctx)
    {
        Console.WriteLine("Running SQLite VACCUUM...");
        ctx.Database.ExecuteSqlRaw("VACUUM");
    }

    private static void BulkLoad<T>(DbContext ctx, IEnumerable<T> entities)
    {
        var propertyNameToGetValue = ReflectionHelper.GetPropertyNameToGetScalarValue<T>();
        var propertyNames = propertyNameToGetValue.Keys.Except(new[] { "Id" }).OrderBy(x => x).ToList();
        var entityType = ctx.Model.FindEntityType(typeof(T))!;
        var tableName = entityType.GetTableName();
        Console.WriteLine($"Loading table {tableName}...");
        var setId = (Action<T, int>)Delegate.CreateDelegate(typeof(Action<T, int>), typeof(T).GetProperty("Id")!.SetMethod!);

        var enumerator = entities.GetEnumerator();
        var batchSize = 10_000;

        var hasMore = true;
        while (hasMore)
        {
            using (var transaction = ctx.Database.BeginTransaction())
            {
                var command = ctx.Database.GetDbConnection().CreateCommand();
                command.CommandText = $"INSERT INTO {tableName} ({string.Join(", ", propertyNames)}) VALUES ({string.Join(", ", propertyNames.Select(x => "@" + x))});";
                var propertyNameToParameter = new Dictionary<string, DbParameter>();
                foreach (var propertyName in propertyNames)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@" + propertyName;
                    command.Parameters.Add(parameter);
                    propertyNameToParameter.Add(propertyName, parameter);
                }

                int count;
                for (count = 0; count < batchSize && (hasMore = enumerator.MoveNext()); count++)
                {
                    var current = enumerator.Current;

                    foreach (var propertyName in propertyNames)
                    {
                        propertyNameToParameter[propertyName].Value = propertyNameToGetValue[propertyName](current) ?? DBNull.Value;
                    }

                    command.ExecuteNonQuery();

                    var idCommand = ctx.Database.GetDbConnection().CreateCommand();
                    idCommand.CommandText = "SELECT last_insert_rowid()";
                    var id = (int)(long)idCommand.ExecuteScalar()!;
                    setId(current, id);
                }

                Console.WriteLine("Saving " + count + "...");
                transaction.Commit();
            }
        }
    }
}
