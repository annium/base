using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

public abstract class CommandBase
{
    internal Root Root => _root ?? throw new InvalidOperationException("Root is not set");
    private Root? _root;

    public abstract Task ProcessAsync(string id, string description, string[] args, CancellationToken ct);

    internal void SetRoot(Root root)
    {
        _root = root;
    }
}
