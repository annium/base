namespace Annium.Data.Operations.Internal;

/// <summary>
/// Internal implementation of a status result with both status and associated data.
/// Represents an operation result that has a status of type TS, carries data of type TD, and can accumulate errors.
/// </summary>
/// <typeparam name="TS">The type of the status value.</typeparam>
/// <typeparam name="TD">The type of data associated with this result.</typeparam>
internal sealed record StatusResult<TS, TD> : ResultBase<IStatusResult<TS, TD>>, IStatusResult<TS, TD>
{
    /// <summary>
    /// Gets the status value of this result.
    /// </summary>
    public TS Status { get; }

    /// <summary>
    /// Gets the data associated with this result.
    /// </summary>
    public TD Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusResult{TS, TD}"/> class.
    /// </summary>
    /// <param name="status">The status value for this result.</param>
    /// <param name="data">The data associated with this result.</param>
    internal StatusResult(TS status, TD data)
    {
        Status = status;
        Data = data;
    }

    /// <summary>
    /// Deconstructs this status result into its status and data components.
    /// </summary>
    /// <param name="status">When this method returns, contains the status value of this result.</param>
    /// <param name="data">When this method returns, contains the data associated with this result.</param>
    public void Deconstruct(out TS status, out TD data)
    {
        status = Status;
        data = Data;
    }

    /// <summary>
    /// Creates a deep copy of this status result, including all errors.
    /// </summary>
    /// <returns>A new <see cref="IStatusResult{TS, TD}"/> instance that is a copy of this result.</returns>
    public override IStatusResult<TS, TD> Copy()
    {
        var clone = new StatusResult<TS, TD>(Status, Data);
        CloneTo(clone);

        return clone;
    }
}

/// <summary>
/// Internal implementation of a status result with status but no associated data.
/// Represents an operation result that has a status of type TS and can accumulate errors.
/// </summary>
/// <typeparam name="TS">The type of the status value.</typeparam>
internal sealed record StatusResult<TS> : ResultBase<IStatusResult<TS>>, IStatusResult<TS>
{
    /// <summary>
    /// Gets the status value of this result.
    /// </summary>
    public TS Status { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusResult{TS}"/> class.
    /// </summary>
    /// <param name="status">The status value for this result.</param>
    internal StatusResult(TS status)
    {
        Status = status;
    }

    /// <summary>
    /// Creates a deep copy of this status result, including all errors.
    /// </summary>
    /// <returns>A new <see cref="IStatusResult{TS}"/> instance that is a copy of this result.</returns>
    public override IStatusResult<TS> Copy()
    {
        var clone = new StatusResult<TS>(Status);
        CloneTo(clone);

        return clone;
    }
}
