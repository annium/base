// ReSharper disable once CheckNamespace

namespace Annium.Extensions.Arguments;

public interface ICommandDescriptor
{
    static abstract string Id { get; }
    static abstract string Description { get; }
}