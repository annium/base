namespace Annium.Data.Operations.Internal;

internal sealed record Result<TD> : ResultBase<IResult<TD>>, IResult<TD>
{
    public TD Data { get; }

    internal Result(TD data)
    {
        Data = data;
    }

    public override IResult<TD> Copy()
    {
        var clone = new Result<TD>(Data);
        CloneTo(clone);

        return clone;
    }
}

internal sealed record Result : ResultBase<IResult>, IResult
{
    public override IResult Copy()
    {
        var clone = new Result();
        CloneTo(clone);

        return clone;
    }
}