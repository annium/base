namespace Annium.Data.Operations.Internal;

internal sealed record StatusResult<TS, TD> : ResultBase<IStatusResult<TS, TD>>, IStatusResult<TS, TD>
{
    public TS Status { get; }

    public TD Data { get; }

    internal StatusResult(TS status, TD data)
    {
        Status = status;
        Data = data;
    }

    public void Deconstruct(out TS status, out TD data)
    {
        status = Status;
        data = Data;
    }

    public override IStatusResult<TS, TD> Copy()
    {
        var clone = new StatusResult<TS, TD>(Status, Data);
        CloneTo(clone);

        return clone;
    }
}

internal sealed record StatusResult<TS> : ResultBase<IStatusResult<TS>>, IStatusResult<TS>
{
    public TS Status { get; }

    internal StatusResult(TS status)
    {
        Status = status;
    }

    public override IStatusResult<TS> Copy()
    {
        var clone = new StatusResult<TS>(Status);
        CloneTo(clone);

        return clone;
    }
}