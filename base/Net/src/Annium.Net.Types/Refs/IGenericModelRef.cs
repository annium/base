namespace Annium.Net.Types.Refs;

public interface IGenericModelRef : IModelRef
{
    IRef[] Args { get; }
}