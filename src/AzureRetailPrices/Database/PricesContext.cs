using Microsoft.EntityFrameworkCore;

namespace Knapcode.AzureRetailPrices.Database;

public class PricesContext : DbContext
{
    public DbSet<Price> Prices => Set<Price>();

    public string DbPath { get; }

    public PricesContext()
    {
        var path = DirectoryHelper.GetRoot();
        DbPath = Path.Join(path, "azure-prices.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");
}
