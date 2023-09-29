namespace Annium.Extensions.Arguments.Internal;

internal interface IConfigurationBuilder
{
    T Build<T>(string[] args)
        where T : new();
}