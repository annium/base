using System;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

public static class Wrap
{
    public static WrappedDelegate It(
        Delegate @delegate,
        [CallerArgumentExpression("delegate")] string delegateEx = default!
    ) => new(@delegate, delegateEx);
}

public readonly ref struct WrappedDelegate
{
    public readonly Delegate Delegate;
    public readonly string DelegateEx;

    public WrappedDelegate(Delegate @delegate, string delegateEx)
    {
        Delegate = @delegate;
        DelegateEx = delegateEx;
    }
}