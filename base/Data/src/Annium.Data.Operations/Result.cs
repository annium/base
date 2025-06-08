using System.Collections.Generic;
using Annium.Data.Operations.Internal;

namespace Annium.Data.Operations;

/// <summary>
/// Factory class for creating result instances
/// </summary>
public static class Result
{
    /// <summary>
    /// Gets an empty result with no errors
    /// </summary>
    public static IResultBase Empty { get; } = new Internal.Result();

    /// <summary>
    /// Joins multiple results by combining their errors
    /// </summary>
    /// <param name="results">The results to join</param>
    /// <returns>A new result containing all combined errors</returns>
    public static IResult Join(params IResultBase[] results) => new Internal.Result().Join(results);

    /// <summary>
    /// Joins multiple results by combining their errors
    /// </summary>
    /// <param name="results">The results to join</param>
    /// <returns>A new result containing all combined errors</returns>
    public static IResult Join(IReadOnlyCollection<IResultBase> results) => new Internal.Result().Join(results);

    /// <summary>
    /// Creates a new empty result
    /// </summary>
    /// <returns>A new empty result</returns>
    public static IResult New() => new Internal.Result();

    /// <summary>
    /// Creates a new result with the specified data
    /// </summary>
    /// <typeparam name="TD">The type of the data</typeparam>
    /// <param name="data">The data to associate with the result</param>
    /// <returns>A new result with the specified data</returns>
    public static IResult<TD> New<TD>(TD data) => new Result<TD>(data);

    /// <summary>
    /// Creates a successful boolean result
    /// </summary>
    /// <returns>A successful boolean result</returns>
    public static IBooleanResult Success() => new BooleanResult(true);

    /// <summary>
    /// Creates a failed boolean result
    /// </summary>
    /// <returns>A failed boolean result</returns>
    public static IBooleanResult Failure() => new BooleanResult(false);

    /// <summary>
    /// Creates a successful boolean result with data
    /// </summary>
    /// <typeparam name="TD">The type of the data</typeparam>
    /// <param name="data">The data to associate with the result</param>
    /// <returns>A successful boolean result with the specified data</returns>
    public static IBooleanResult<TD> Success<TD>(TD data) => new BooleanResult<TD>(true, data);

    /// <summary>
    /// Creates a failed boolean result with data
    /// </summary>
    /// <typeparam name="TD">The type of the data</typeparam>
    /// <param name="data">The data to associate with the result</param>
    /// <returns>A failed boolean result with the specified data</returns>
    public static IBooleanResult<TD> Failure<TD>(TD data) => new BooleanResult<TD>(false, data);

    /// <summary>
    /// Creates a status result with the specified status
    /// </summary>
    /// <typeparam name="TS">The type of the status</typeparam>
    /// <param name="status">The status value</param>
    /// <returns>A status result with the specified status</returns>
    public static IStatusResult<TS> Status<TS>(TS status) => new StatusResult<TS>(status);

    /// <summary>
    /// Creates a status result with the specified status and data
    /// </summary>
    /// <typeparam name="TS">The type of the status</typeparam>
    /// <typeparam name="TD">The type of the data</typeparam>
    /// <param name="status">The status value</param>
    /// <param name="data">The data to associate with the result</param>
    /// <returns>A status result with the specified status and data</returns>
    public static IStatusResult<TS, TD> Status<TS, TD>(TS status, TD data) => new StatusResult<TS, TD>(status, data);
}
