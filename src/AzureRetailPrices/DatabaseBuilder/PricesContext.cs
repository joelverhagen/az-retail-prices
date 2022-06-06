using Microsoft.EntityFrameworkCore;

namespace Knapcode.AzureRetailPrices;

public class PricesContext : DbContext
{
    public DbSet<ArmRegion> ArmRegions => Set<ArmRegion>();
    public DbSet<ArmSku> ArmSkus => Set<ArmSku>();

    public string DbPath { get; }

    public PricesContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "azure-prices.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");
}
