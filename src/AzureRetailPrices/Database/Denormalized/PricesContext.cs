using Microsoft.EntityFrameworkCore;

namespace Knapcode.AzureRetailPrices.Database.Denormalized;

public class PricesContext : DbContext
{
    public DbSet<Price> Prices => Set<Price>();

    public string DbPath { get; }

    public PricesContext()
    {
        var path = DirectoryHelper.GetRoot();
        DbPath = Path.Join(path, "azure-prices-denormalized.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Price>().HasIndex(x => new { x.MeterId, x.MeterName, x.PriceType, x.SkuId, x.TierMinimumUnits }).IsUnique();
        modelBuilder.Entity<Price>().HasIndex(x => new { x.ArmRegionName });
        modelBuilder.Entity<Price>().HasIndex(x => new { x.MeterName });
        modelBuilder.Entity<Price>().HasIndex(x => new { x.SkuId });
        modelBuilder.Entity<Price>().HasIndex(x => new { x.ProductName });
        modelBuilder.Entity<Price>().HasIndex(x => new { x.ServiceName });
        modelBuilder.Entity<Price>().HasIndex(x => new { x.ServiceFamily });
        modelBuilder.Entity<Price>().HasIndex(x => new { x.ArmSkuName });
    }
}
