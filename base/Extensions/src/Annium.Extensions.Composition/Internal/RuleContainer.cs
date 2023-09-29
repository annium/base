using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Annium.Extensions.Composition.Internal;

internal class RuleContainer<TValue, TField> : IRuleBuilder<TValue, TField>, IRuleContainer<TValue>
{
    private readonly MethodInfo _setTarget;
    private Delegate? _check;
    private Delegate? _load;
    private string _message = string.Empty;
    private bool _allowDefault;

    public RuleContainer(
        MethodInfo setTarget,
        bool allowDefault
    )
    {
        _setTarget = setTarget;
        _allowDefault = allowDefault;
    }

    public IRuleBuilder<TValue, TField> When(Func<CompositionContext<TValue>, bool> check)
    {
        _check = check;

        return this;
    }

    public IRuleBuilder<TValue, TField> When(Func<CompositionContext<TValue>, Task<bool>> check)
    {
        _check = check;

        return this;
    }

    public void LoadWith(
        Func<CompositionContext<TValue>, TField?> load,
        string message = ""
    )
    {
        _load = load;
        _message = message;
    }

    public void LoadWith(
        Func<CompositionContext<TValue>, Task<TField?>> load,
        string message = ""
    )
    {
        _load = load;
        _message = message;
    }

    public async Task ComposeAsync(CompositionContext<TValue> context, TValue value)
    {
        if (!await CheckAsync(context))
            return;

        var target = await LoadAsync(context);

        if ((target is null || target.Equals(default(TField)!)) && !_allowDefault)
            context.Error(string.IsNullOrEmpty(_message) ? "{0} not found" : _message, context.Field);
        else
            _setTarget.Invoke(value, new object[] { target! });
    }

    private async Task<bool> CheckAsync(CompositionContext<TValue> context) => _check switch
    {
        Func<CompositionContext<TValue>, bool> check       => check(context),
        Func<CompositionContext<TValue>, Task<bool>> check => await check(context),
        _                                                  => true,
    };

    private async Task<TField?> LoadAsync(CompositionContext<TValue> context) => _load switch
    {
        Func<CompositionContext<TValue>, TField?> load       => load(context),
        Func<CompositionContext<TValue>, Task<TField?>> load => await load(context),
        _                                                    => throw new InvalidOperationException($"{context.Field} has no {nameof(LoadWith)} defined."),
    };
}