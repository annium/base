namespace Annium.Data.Operations.Internal;

/// <summary>
/// Internal implementation of a boolean result with associated data.
/// Represents an operation result that has a success/failure status and carries data of type TD.
/// </summary>
/// <typeparam name="TD">The type of data associated with this result.</typeparam>
internal sealed record BooleanResult<TD> : ResultBase<IBooleanResult<TD>>, IBooleanResult<TD>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the data associated with this result.
    /// </summary>
    public TD Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanResult{TD}"/> class.
    /// </summary>
    /// <param name="value">A value indicating whether the operation was successful.</param>
    /// <param name="data">The data associated with this result.</param>
    internal BooleanResult(bool value, TD data)
    {
        IsSuccess = value;
        Data = data;
    }

    /// <summary>
    /// Deconstructs this boolean result into its success status and data components.
    /// </summary>
    /// <param name="succeed">When this method returns, contains the success status of the operation.</param>
    /// <param name="data">When this method returns, contains the data associated with this result.</param>
    public void Deconstruct(out bool succeed, out TD data)
    {
        succeed = IsSuccess;
        data = Data;
    }

    /// <summary>
    /// Creates a deep copy of this boolean result, including all errors.
    /// </summary>
    /// <returns>A new <see cref="IBooleanResult{TD}"/> instance that is a copy of this result.</returns>
    public override IBooleanResult<TD> Copy()
    {
        var clone = new BooleanResult<TD>(IsSuccess, Data);
        CloneTo(clone);

        return clone;
    }
}

/// <summary>
/// Internal implementation of a boolean result without associated data.
/// Represents an operation result that has only a success/failure status.
/// </summary>
internal sealed record BooleanResult : ResultBase<IBooleanResult>, IBooleanResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanResult"/> class.
    /// </summary>
    /// <param name="value">A value indicating whether the operation was successful.</param>
    internal BooleanResult(bool value)
    {
        IsSuccess = value;
    }

    /// <summary>
    /// Creates a deep copy of this boolean result, including all errors.
    /// </summary>
    /// <returns>A new <see cref="IBooleanResult"/> instance that is a copy of this result.</returns>
    public override IBooleanResult Copy()
    {
        var clone = new BooleanResult(IsSuccess);
        CloneTo(clone);

        return clone;
    }
}
