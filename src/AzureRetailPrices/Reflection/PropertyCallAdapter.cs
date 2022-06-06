namespace Knapcode.AzureRetailPrices.Reflection;

/// <summary>
/// Source: https://stackoverflow.com/a/26733318
/// </summary>
public class PropertyCallAdapter<TThis, TResult> : IPropertyCallAdapter<TThis>
{
    private readonly Func<TThis, TResult> _getterInvocation;

    public PropertyCallAdapter(Func<TThis, TResult> getterInvocation)
    {
        _getterInvocation = getterInvocation;
    }

    public object? InvokeGet(TThis @this)
    {
        return _getterInvocation.Invoke(@this);
    }
}
