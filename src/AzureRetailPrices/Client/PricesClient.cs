using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OData.Client;

namespace Knapcode.AzureRetailPrices.Client;

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

        using var response = await GetPricesPageAsync(currencyCode, onlyPrimaryMeterRegion, query);
        foreach (var item in response.Value.Items)
        {
            yield return item;
        }

        string? nextPageLink = response.Value.NextPageLink;

        while (nextPageLink != null)
        {
            using var nextResponse = await GetPricesPageAsync(nextPageLink);
            foreach (var item in nextResponse.Value.Items)
            {
                yield return item;
            }

            nextPageLink = nextResponse.Value.NextPageLink;
        }
    }

    public async Task<JsonResponse<PricesResponse>> GetPricesPageAsync(
        string? currencyCode = null,
        bool? onlyPrimaryMeterRegion = null,
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

        if (onlyPrimaryMeterRegion.HasValue)
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

        return await GetPricesPageAsync(url);
    }

    private async Task<JsonResponse<PricesResponse>> GetPricesPageAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("User-Agent", "AzureRetailPrices/0.0.1 (.NET; +https://github.com/joelverhagen/data-az-retail-prices)");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync();
        var document = JsonDocument.Parse(stream);
        try
        {
            var pricesResponse = JsonSerializer.Deserialize<PricesResponse>(document);
            if (pricesResponse is null)
            {
                throw new InvalidDataException("The prices response is null.");
            }

            return new (document, pricesResponse);
        }
        catch
        {
            document.Dispose();
            throw;
        }
    }

    private class PricesDataServiceContext : DataServiceContext
    {
        private const string Url = "http://localhost/Prices.svc";

        public PricesDataServiceContext() : base(new Uri(Url))
        {
        }

        public DataServiceQuery<PriceFilter> Filters => CreateQuery<PriceFilter>("Filters");
    }
}
