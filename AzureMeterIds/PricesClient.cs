using System;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OData.Client;

public class PricesClient
{
    private static readonly PricesDataServiceContext FilterBuilder = new();

    private readonly HttpClient _httpClient;

    public PricesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async IAsyncEnumerable<PriceResponse> GetPricesAsync(
        string? currencyCode = null,
        bool onlyPrimaryMeterRegion = false,
        Func<IQueryable<PriceFilter>, IQueryable<PriceFilter>>? query = null)
    {
        var response = await GetPricesPageAsync(currencyCode, onlyPrimaryMeterRegion, query);
        foreach (var item in response.Items)
        {
            yield return item;
        }

        while (response.NextPageLink is not null)
        {
            response = await GetPricesResponseAsync(response.NextPageLink);
            foreach (var item in response.Items)
            {
                yield return item;
            }
        }
    }

    public async Task<PricesResponse> GetPricesPageAsync(
        string? currencyCode = null,
        bool onlyPrimaryMeterRegion = false,
        Func<IQueryable<PriceFilter>, IQueryable<PriceFilter>>? query = null)
    {
        var baseUrl = "https://prices.azure.com/api/retail/prices";

        var queryString = new SortedDictionary<string, string>
        {
            { "api-version", "2021-10-01-preview" },
        };

        if (currencyCode is not null)
        {
            queryString["currencyCode"] = currencyCode;
        }

        if (onlyPrimaryMeterRegion)
        {
            queryString["meterRegion"] = "primary";
        }

        if (query is not null)
        {
            var dataServiceQuery = (DataServiceQuery<PriceFilter>)query(FilterBuilder.Filters);
            var dataServiceQueryString = QueryHelpers.ParseQuery(dataServiceQuery.RequestUri.Query);

            foreach (var pair in dataServiceQueryString)
            {
                if (pair.Value.Count > 0)
                {
                    queryString[pair.Key] = pair.Value.Last();
                }
            }
        }

        var url = QueryHelpers.AddQueryString(baseUrl, queryString);

        return await GetPricesResponseAsync(url);
    }

    private async Task<PricesResponse> GetPricesResponseAsync(string url)
    {
        Console.WriteLine(url);

        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        using var stream = response.Content.ReadAsStream();
        var pricesResponse = JsonSerializer.Deserialize<PricesResponse>(stream);
        if (pricesResponse is null)
        {
            throw new InvalidDataException("The prices response is null.");
        }

        return pricesResponse;
    }

    private class PricesDataServiceContext : DataServiceContext
    {
        private const string url = "http://localhost/Prices.svc";

        public PricesDataServiceContext() : base(new Uri(url))
        {
        }

        public DataServiceQuery<PriceFilter> Filters => CreateQuery<PriceFilter>("Filters");
    }
}
