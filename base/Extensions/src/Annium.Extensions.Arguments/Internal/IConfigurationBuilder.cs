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

    /// <summary>
    /// Determines whether the command line asks for help, without binding it to anything.
    /// </summary>
    /// <param name="args">Array of command line arguments to inspect</param>
    /// <returns>True when help was asked for</returns>
    /// <remarks>
    /// This runs over arguments belonging to whatever command is about to handle them, so it cannot go
    /// through <see cref="Build{T}"/>: a type that accepts only the help flag would reject everything else
    /// the command legitimately takes.
    /// </remarks>
    bool IsHelpRequested(string[] args);
}
