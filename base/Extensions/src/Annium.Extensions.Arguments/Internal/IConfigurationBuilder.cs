using System;

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

    /// <summary>
    /// Fails when a command's configuration types would not read the given command line the same way.
    /// </summary>
    /// <param name="args">Array of command line arguments the command was given</param>
    /// <param name="configurationTypes">Every configuration type the command binds</param>
    /// <remarks>
    /// Each type is bound from the same arguments using only its own options in view, so a flag declared by
    /// one of them is an unknown option to another, which swallows the token after it. This judges the
    /// command's configuration, not the input: if the types cannot agree on what they were given, whatever
    /// they bind is arbitrary, and saying so beats handing back two disagreeing readings.
    /// </remarks>
    void EnsureTypesReadAlike(string[] args, params Type[] configurationTypes);
}
