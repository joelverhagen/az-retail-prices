using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Knapcode.AzureRetailPrices.Client;
using Knapcode.AzureRetailPrices.Database;
using Knapcode.AzureRetailPrices.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Knapcode.AzureRetailPrices.LoadDatabase
{
    public static class LoadDatabaseCommand
    {
        public static void Run()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();

            using var ctx = new PricesContext();
            ctx.Database.EnsureCreatedAsync();

            var propertyNameToGetValue = ReflectionHelper.GetPropertyNameToGetValue<Price>();
            var propertyNames = propertyNameToGetValue.Keys.Except(new[] { nameof(Price.PriceId) }).OrderBy(x => x).ToList();

            var prices = DirectoryHelper.GetPricesFromLatestSnapshot();
            var enumerator = prices.GetEnumerator();
            var batchSize = 10_000;

            var hasMore = true;
            while (hasMore)
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    var command = ctx.Database.GetDbConnection().CreateCommand();
                    command.CommandText = $"INSERT INTO Prices ({string.Join(", ", propertyNames)}) VALUES ({string.Join(", ", propertyNames.Select(x => "@" + x))});";
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
                        var price = mapper.Map<Price>(enumerator.Current);

                        foreach (var propertyName in propertyNames)
                        {
                            propertyNameToParameter[propertyName].Value = propertyNameToGetValue[propertyName](price) ?? DBNull.Value;
                        }

                        command.ExecuteNonQuery();
                    }

                    Console.WriteLine("Saving " + count + "...");
                    transaction.Commit();
                }
            }
        }
    }
}
