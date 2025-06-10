namespace Annium.Data.Operations;

/// <summary>
/// Represents a status result with associated data
/// </summary>
/// <typeparam name="TS">The type of the status</typeparam>
/// <typeparam name="TD">The type of the associated data</typeparam>
public interface IStatusResult<TS, TD> : IResultBase<IStatusResult<TS, TD>>, IDataResultBase<TD>
{
    /// <summary>
    /// Gets the status of the operation
    /// </summary>
    TS Status { get; }

    /// <summary>
    /// Deconstructs the result into status and data
    /// </summary>
    /// <param name="status">The operation status</param>
    /// <param name="data">The associated data</param>
    void Deconstruct(out TS status, out TD data);
}

/// <summary>
/// Represents a status result without associated data
/// </summary>
/// <typeparam name="TS">The type of the status</typeparam>
public interface IStatusResult<TS> : IResultBase<IStatusResult<TS>>, IResultBase
{
    /// <summary>
    /// Gets the status of the operation
    /// </summary>
    TS Status { get; }
}
