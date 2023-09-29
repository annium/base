namespace Annium.Extensions.Pooling.Internal.Loaders;

internal interface ILoader<T>
{
    T Get();
}