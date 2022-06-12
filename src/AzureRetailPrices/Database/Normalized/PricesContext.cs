using Microsoft.EntityFrameworkCore;

namespace Knapcode.AzureRetailPrices.Database.Normalized;

public class PricesContext : DbContext
{
    public DbSet<ArmSkuName> ArmSkuNames => Set<ArmSkuName>();
    public DbSet<Meter> Meters => Set<Meter>();
    public DbSet<MeterName> MeterNames => Set<MeterName>();
    public DbSet<Price> Prices => Set<Price>();
    public DbSet<PriceType> PriceTypes => Set<PriceType>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<ReservationTerm> ReservationTerms => Set<ReservationTerm>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceFamily> ServiceFamilies => Set<ServiceFamily>();
    public DbSet<Sku> Skus => Set<Sku>();
    public DbSet<SkuName> SkuNames => Set<SkuName>();
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();

    public string DbPath { get; }

    public PricesContext()
    {
        var path = DirectoryHelper.GetRoot();
        DbPath = Path.Join(path, "azure-prices-normalized.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArmSkuName>().HasIndex(x => new { x.Value }).IsUnique();
        modelBuilder.Entity<Meter>().HasIndex(x => new { x.MeterId }).IsUnique();
        modelBuilder.Entity<MeterName>().HasIndex(x => new { x.Value }).IsUnique();
        modelBuilder.Entity<Price>().HasIndex(x => new { x.MeterId, x.MeterNameId, x.PriceTypeId, x.SkuId, x.TierMinimumUnits }).IsUnique();
        modelBuilder.Entity<PriceType>().HasIndex(x => new { x.Value }).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(x => new { x.ProductId }).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(x => new { x.ProductName }).IsUnique();
        modelBuilder.Entity<Region>().HasIndex(x => new { x.ArmRegionName }).IsUnique();
        modelBuilder.Entity<Region>().HasIndex(x => new { x.Location }).IsUnique();
        modelBuilder.Entity<ReservationTerm>().HasIndex(x => new { x.Value }).IsUnique();
        modelBuilder.Entity<Service>().HasIndex(x => new { x.ServiceId }).IsUnique();
        modelBuilder.Entity<Service>().HasIndex(x => new { x.ServiceName }).IsUnique();
        modelBuilder.Entity<ServiceFamily>().HasIndex(x => new { x.Value }).IsUnique();
        modelBuilder.Entity<Sku>().HasIndex(x => new { x.ProductId, x.SkuIdSuffix }).IsUnique();
        modelBuilder.Entity<SkuName>().HasIndex(x => new { x.Value }).IsUnique();
        modelBuilder.Entity<UnitOfMeasure>().HasIndex(x => new { x.Value }).IsUnique();
    }
}
