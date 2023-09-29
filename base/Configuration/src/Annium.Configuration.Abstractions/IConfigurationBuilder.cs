namespace Annium.Configuration.Abstractions;

public interface IConfigurationBuilder : IConfigurationContainer
{
    T Build<T>() where T : new();
}