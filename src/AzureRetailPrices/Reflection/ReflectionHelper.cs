using System.Reflection;
using Knapcode.AzureRetailPrices.Client;

namespace Knapcode.AzureRetailPrices.Reflection;

public static class ReflectionHelper
{
    public static Dictionary<string, Func<T, object?>> GetPropertyNameToGetValue<T>()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            .ToDictionary<PropertyInfo, string, Func<T, object?>>(
                x => x.Name,
                x => y => PropertyCallAdapterProvider<T>.GetInstance(x.Name).InvokeGet(y));
    }

    public static Dictionary<string, Func<T, string?>> GetPropertyNameToGetStringValue<T>()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            .Where(x => x.PropertyType == typeof(string))
            .ToDictionary(
                x => x.Name,
                x => (Func<T, string?>)Delegate.CreateDelegate(typeof(Func<T, string?>), x.GetMethod!));
    }
}
