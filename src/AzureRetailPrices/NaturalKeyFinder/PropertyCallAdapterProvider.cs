using System.Reflection;

namespace Knapcode.AzureRetailPrices;

/// <summary>
/// Source: https://stackoverflow.com/a/26733318
/// </summary>
public class PropertyCallAdapterProvider<TThis>
{
    private static readonly Dictionary<string, IPropertyCallAdapter<TThis>> _instances = new();

    public static IPropertyCallAdapter<TThis> GetInstance(string forPropertyName)
    {
        if (!_instances.TryGetValue(forPropertyName, out var instance))
        {
            lock (_instances)
            {
                if (!_instances.TryGetValue(forPropertyName, out instance))
                {
                    var property = typeof(TThis).GetProperty(
                    forPropertyName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    MethodInfo? getMethod;
                    Delegate? getterInvocation = null;
                    if (property != null && (getMethod = property.GetGetMethod(true)) != null)
                    {
                        var openGetterType = typeof(Func<,>);
                        var concreteGetterType = openGetterType
                            .MakeGenericType(typeof(TThis), property.PropertyType);

                        getterInvocation =
                            Delegate.CreateDelegate(concreteGetterType, null, getMethod);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    var openAdapterType = typeof(PropertyCallAdapter<,>);
                    var concreteAdapterType = openAdapterType
                        .MakeGenericType(typeof(TThis), property.PropertyType);
                    instance = (IPropertyCallAdapter<TThis>)Activator.CreateInstance(concreteAdapterType, getterInvocation)!;

                    _instances.Add(forPropertyName, instance);
                }
            }
        }

        return instance;
    }
}
