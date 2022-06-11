using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Knapcode.AzureRetailPrices.Client;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Knapcode.AzureRetailPrices.Snapshot;

public static class SnapshotCommand
{
    public static async Task RunAsync()
    {
        // Clear the snapshot directory
        var dir = DirectoryHelper.GetSnapshotDirectory();
        var lastCount = 0;
        if (Directory.Exists(dir))
        {
            lastCount = DirectoryHelper.GetLatestSnapshotFiles().Count();
            Directory.Delete(dir, recursive: true);
        }

        Directory.CreateDirectory(dir);

        // Start downloading the pages
        var services = new ServiceCollection();

        services
            .AddHttpClient<PricesClient>()
            .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
            })
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(3)));

        var serviceProvider = services.BuildServiceProvider();

        var client = serviceProvider.GetRequiredService<PricesClient>();
        var take = 100;
        var workerCount = 16;
        var sw = Stopwatch.StartNew();
        var progress = new Progress(lastCount);
        var workers = Enumerable
            .Range(0, workerCount)
            .Select(x => SnapshotAsync(dir, client, x, workerCount, take, progress))
            .ToList();
        await Task.WhenAll(workers);
    }

    private static async Task SnapshotAsync(string dir, PricesClient client, int firstPage, int pageSkip, int take, Progress progress)
    {
        var currentPage = firstPage;

        while (true)
        {
            var sw = Stopwatch.StartNew();
            using var response = await client.GetPricesPageAsync(
                currencyCode: "USD",
                onlyPrimaryMeterRegion: false,
                query: p => p
                    .OrderBy(x => x.MeterId)
                    .ThenBy(x => x.MeterName)
                    .ThenBy(x => x.PriceType)
                    .ThenBy(x => x.SkuId)
                    .ThenBy(x => x.TierMinimumUnits)
                    .Skip(currentPage * take)
                    .Take(take));
            progress.Increment();
            Console.WriteLine($"[{progress.Total.TotalSeconds,9:F3}] [Worker {firstPage:D2}] Fetched page {currentPage:D5} / {progress.LastCount:D5} in {sw.Elapsed.TotalSeconds,6:F3}s (rate: {progress.RateSeconds:F3}/s)");

            if (response.Value.Items.Length == 0)
            {
                break;
            }

            var path = Path.Combine(dir, $"page{currentPage:D5}.json");
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            response.Document.WriteTo(writer);

            currentPage += pageSkip;
        }
    }

    private class Progress
    {
        private readonly Stopwatch _sw;
        private int _completed;

        public Progress(int lastCount)
        {
            _sw = Stopwatch.StartNew();
            LastCount = lastCount;
        }

        public TimeSpan Total => _sw.Elapsed;
        public int LastCount { get; }
        public int Completed => _completed;
        public double RateSeconds => Completed / Total.TotalSeconds;

        public void Increment()
        {
            Interlocked.Increment(ref _completed);
        }
    }
}
