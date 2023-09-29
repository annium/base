namespace Annium.Net.Types.Refs;

public interface IModelRef : IRef
{
    string Namespace { get; }
    string Name { get; }
}