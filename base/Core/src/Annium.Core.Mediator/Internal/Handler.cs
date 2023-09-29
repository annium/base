using System;

namespace Annium.Core.Mediator.Internal;

internal record Handler
{
    public Type Implementation { get; }
    public Type RequestIn { get; }
    public Type? RequestOut { get; }
    public Type? ResponseIn { get; }
    public Type ResponseOut { get; }

    internal Handler(
        Type implementation,
        Type requestIn,
        Type? requestOut,
        Type? responseIn,
        Type responseOut
    )
    {
        Implementation = implementation;
        RequestIn = requestIn;
        RequestOut = requestOut;
        ResponseIn = responseIn;
        ResponseOut = responseOut;
    }

    public override string ToString() => Implementation.ToString();
}