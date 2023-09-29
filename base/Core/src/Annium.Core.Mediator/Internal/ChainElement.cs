using System;

namespace Annium.Core.Mediator.Internal;

internal record ChainElement
{
    public Type Handler { get; }
    public Delegate? Next { get; }

    public ChainElement(
        Type handler
    )
    {
        Handler = handler;
    }

    public ChainElement(
        Type handler,
        Delegate next
    ) : this(handler)
    {
        Next = next;
    }

    public override string ToString() => Handler.ToString();
}