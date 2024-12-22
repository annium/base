using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Testing;

public static class Wrap
{
    public static WrappedAction It(
        Action action,
        [CallerArgumentExpression(nameof(action))] string delegateEx = default!
    ) => new(action, delegateEx);

    public static WrappedTaskAction It(
        Func<Task> action,
        [CallerArgumentExpression(nameof(action))] string delegateEx = default!
    ) => new(action, delegateEx);
}

public readonly struct WrappedAction
{
    public readonly Action Execute;
    public readonly string Expression;

    public WrappedAction(Action execute, string expression)
    {
        Execute = execute;
        Expression = expression;
    }
}

public readonly struct WrappedTaskAction
{
    public readonly Func<Task> Execute;
    public readonly string Expression;

    public WrappedTaskAction(Func<Task> execute, string expression)
    {
        Execute = execute;
        Expression = expression;
    }
}
