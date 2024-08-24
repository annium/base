namespace Annium.Data.Operations.Internal;

internal sealed record BooleanResult<TD> : ResultBase<IBooleanResult<TD>>, IBooleanResult<TD>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public TD Data { get; }

    internal BooleanResult(bool value, TD data)
    {
        IsSuccess = value;
        Data = data;
    }

    public void Deconstruct(out bool succeed, out TD data)
    {
        succeed = IsSuccess;
        data = Data;
    }

    public override IBooleanResult<TD> Copy()
    {
        var clone = new BooleanResult<TD>(IsSuccess, Data);
        CloneTo(clone);

        return clone;
    }
}

internal sealed record BooleanResult : ResultBase<IBooleanResult>, IBooleanResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    internal BooleanResult(bool value)
    {
        IsSuccess = value;
    }

    public override IBooleanResult Copy()
    {
        var clone = new BooleanResult(IsSuccess);
        CloneTo(clone);

        return clone;
    }
}
