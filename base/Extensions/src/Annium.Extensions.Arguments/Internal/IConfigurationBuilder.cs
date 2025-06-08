namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Defines the contract for building typed configuration objects from command line arguments.
/// </summary>
internal interface IConfigurationBuilder
{
    /// <summary>
    /// Builds a typed configuration object from command line arguments.
    /// </summary>
    /// <typeparam name="T">The configuration type to build, must have a parameterless constructor</typeparam>
    /// <param name="args">Array of command line arguments to process</param>
    /// <returns>A fully populated configuration object of type T</returns>
    T Build<T>(string[] args)
        where T : new();
}
