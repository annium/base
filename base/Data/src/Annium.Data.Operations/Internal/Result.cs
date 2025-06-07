namespace Annium.Data.Operations.Internal;

/// <summary>
/// Internal implementation of a result with associated data.
/// Represents an operation result that carries data of type TD and can accumulate errors.
/// </summary>
/// <typeparam name="TD">The type of data associated with this result.</typeparam>
internal sealed record Result<TD> : ResultBase<IResult<TD>>, IResult<TD>
{
    /// <summary>
    /// Gets the data associated with this result.
    /// </summary>
    public TD Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TD}"/> class.
    /// </summary>
    /// <param name="data">The data associated with this result.</param>
    internal Result(TD data)
    {
        Data = data;
    }

    /// <summary>
    /// Creates a deep copy of this result, including all errors.
    /// </summary>
    /// <returns>A new <see cref="IResult{TD}"/> instance that is a copy of this result.</returns>
    public override IResult<TD> Copy()
    {
        var clone = new Result<TD>(Data);
        CloneTo(clone);

        return clone;
    }
}

/// <summary>
/// Internal implementation of a result without associated data.
/// Represents an operation result that can accumulate errors but does not carry data.
/// </summary>
internal sealed record Result : ResultBase<IResult>, IResult
{
    /// <summary>
    /// Creates a deep copy of this result, including all errors.
    /// </summary>
    /// <returns>A new <see cref="IResult"/> instance that is a copy of this result.</returns>
    public override IResult Copy()
    {
        var clone = new Result();
        CloneTo(clone);

        return clone;
    }
}
