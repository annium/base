namespace Annium.Data.Operations.Internal;

internal sealed record BooleanResult<TD> : ResultBase<IBooleanResult<TD>>, IBooleanResult<TD>
{
    public bool IsSuccess => _value;
    public bool IsFailure => !_value;
    public TD Data { get; }
    private readonly bool _value;

    internal BooleanResult(bool value, TD data)
    {
        _value = value;
        Data = data;
    }

    public void Deconstruct(out bool succeed, out TD data)
    {
        succeed = IsSuccess;
        data = Data;
    }

    public override IBooleanResult<TD> Copy()
    {
        var clone = new BooleanResult<TD>(_value, Data);
        CloneTo(clone);

        return clone;
    }
}

internal sealed record BooleanResult : ResultBase<IBooleanResult>, IBooleanResult
{
    public bool IsSuccess => _value;
    public bool IsFailure => !_value;
    private readonly bool _value;

    internal BooleanResult(bool value)
    {
        _value = value;
    }

    public override IBooleanResult Copy()
    {
        var clone = new BooleanResult(_value);
        CloneTo(clone);

        return clone;
    }
}