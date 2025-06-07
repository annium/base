using System;
using System.Collections.Generic;

namespace Annium.Data.Operations;

/// <summary>
/// Base interface for results that can be manipulated
/// </summary>
/// <typeparam name="T">The type of result that operations return</typeparam>
public interface IResultBase<T> : ICloneableResultBase<T>
{
    /// <summary>
    /// Clears all errors from the result
    /// </summary>
    /// <returns>The result instance for method chaining</returns>
    T Clear();

    /// <summary>
    /// Adds an error to the result
    /// </summary>
    /// <param name="error">The error message to add</param>
    /// <returns>The result instance for method chaining</returns>
    T Error(string error);

    /// <summary>
    /// Adds a labeled error to the result
    /// </summary>
    /// <param name="label">The label for the error</param>
    /// <param name="error">The error message to add</param>
    /// <returns>The result instance for method chaining</returns>
    T Error(string label, string error);

    /// <summary>
    /// Adds multiple errors to the result
    /// </summary>
    /// <param name="errors">The error messages to add</param>
    /// <returns>The result instance for method chaining</returns>
    T Errors(params string[] errors);

    /// <summary>
    /// Adds multiple errors to the result
    /// </summary>
    /// <param name="errors">The error messages to add</param>
    /// <returns>The result instance for method chaining</returns>
    T Errors(IReadOnlyCollection<string> errors);

    /// <summary>
    /// Adds multiple labeled errors to the result
    /// </summary>
    /// <param name="errors">The labeled error messages to add</param>
    /// <returns>The result instance for method chaining</returns>
    T Errors(params ValueTuple<string, IReadOnlyCollection<string>>[] errors);

    /// <summary>
    /// Adds multiple labeled errors to the result
    /// </summary>
    /// <param name="errors">The labeled error messages to add</param>
    /// <returns>The result instance for method chaining</returns>
    T Errors(IReadOnlyCollection<KeyValuePair<string, IReadOnlyCollection<string>>> errors);

    /// <summary>
    /// Joins multiple results by combining their errors
    /// </summary>
    /// <param name="results">The results to join</param>
    /// <returns>The result instance for method chaining</returns>
    T Join(params IResultBase[] results);

    /// <summary>
    /// Joins multiple results by combining their errors
    /// </summary>
    /// <param name="results">The results to join</param>
    /// <returns>The result instance for method chaining</returns>
    T Join(IReadOnlyCollection<IResultBase> results);
}

/// <summary>
/// Interface for results that can be copied
/// </summary>
/// <typeparam name="T">The type of result that copy operations return</typeparam>
public interface ICloneableResultBase<T>
{
    /// <summary>
    /// Creates a copy of the result
    /// </summary>
    /// <returns>A copy of the result</returns>
    T Copy();
}

/// <summary>
/// Base interface for results that contain data
/// </summary>
/// <typeparam name="TD">The type of the data</typeparam>
public interface IDataResultBase<TD> : IResultBase
{
    /// <summary>
    /// Gets the data associated with the result
    /// </summary>
    TD Data { get; }
}

/// <summary>
/// Base interface for all results
/// </summary>
public interface IResultBase
{
    /// <summary>
    /// Gets the collection of plain (unlabeled) errors
    /// </summary>
    IReadOnlyCollection<string> PlainErrors { get; }

    /// <summary>
    /// Gets the first plain error, or empty string if none
    /// </summary>
    string PlainError { get; }

    /// <summary>
    /// Gets the dictionary of labeled errors
    /// </summary>
    IReadOnlyDictionary<string, IReadOnlyCollection<string>> LabeledErrors { get; }

    /// <summary>
    /// Gets a value indicating whether the result has no errors
    /// </summary>
    bool IsOk { get; }

    /// <summary>
    /// Gets a value indicating whether the result has any errors
    /// </summary>
    bool HasErrors { get; }

    /// <summary>
    /// Gets a string representation of all errors
    /// </summary>
    /// <returns>A formatted string containing all errors</returns>
    string ErrorState();
}
