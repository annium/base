using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Composition;

public class CompositionContext<TValue>
{
    public TValue Root { get; }
    public string Label { get; }
    public string Field { get; }
    private readonly IResult _result;
    private readonly ILocalizer _localizer;

    internal CompositionContext(
        TValue root,
        string label,
        string field,
        IResult result,
        ILocalizer localizer
    )
    {
        Root = root;
        Label = label;
        Field = field;
        _result = result;
        _localizer = localizer;
    }

    public void Error(string error, params object[] arguments)
    {
        _result.Error(Label, _localizer[error, arguments]);
    }
}