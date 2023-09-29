namespace Annium.Data.Operations;

public interface IStatusResult<TS, TD> : IResultBase<IStatusResult<TS, TD>>, IDataResultBase<TD>
{
    TS Status { get; }

    void Deconstruct(out TS status, out TD data);
}

public interface IStatusResult<TS> : IResultBase<IStatusResult<TS>>, IResultBase
{
    TS Status { get; }
}