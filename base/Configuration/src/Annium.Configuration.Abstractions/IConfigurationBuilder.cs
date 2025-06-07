namespace Annium.Configuration.Abstractions;

/// <summary>
/// Interface for building configuration objects from configuration data
/// </summary>
public interface IConfigurationBuilder : IConfigurationContainer
{
    /// <summary>
    /// Builds an instance of type T from the configuration data
    /// </summary>
    /// <returns>Configured instance of type T</returns>
    T Build<T>()
        where T : new();
}
