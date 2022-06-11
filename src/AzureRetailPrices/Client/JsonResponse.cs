using System.Text.Json;

namespace Knapcode.AzureRetailPrices.Client;

public class JsonResponse<T> : IDisposable
{
    public JsonResponse(JsonDocument document, T value)
    {
        Document = document;
        Value = value;
    }

    public JsonDocument Document { get; set; }
    public T Value { get; set; }

    public void Dispose()
    {
        Document?.Dispose();
    }
}
