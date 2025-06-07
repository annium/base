namespace Annium.Data.Operations;

/// <summary>
/// Represents a boolean result with associated data
/// </summary>
/// <typeparam name="TD">The type of the associated data</typeparam>
public interface IBooleanResult<TD> : IResultBase<IBooleanResult<TD>>, IDataResultBase<TD>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Deconstructs the result into success flag and data
    /// </summary>
    /// <param name="succeed">Whether the operation succeeded</param>
    /// <param name="data">The associated data</param>
    void Deconstruct(out bool succeed, out TD data);
}

/// <summary>
/// Represents a boolean result without associated data
/// </summary>
public interface IBooleanResult : IResultBase<IBooleanResult>, IResultBase
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed
    /// </summary>
    bool IsFailure { get; }
}
