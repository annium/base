namespace Annium.Data.Operations;

public interface IBooleanResult<TD> : IResultBase<IBooleanResult<TD>>, IDataResultBase<TD>
{
    bool IsSuccess { get; }
    bool IsFailure { get; }

    void Deconstruct(out bool succeed, out TD data);
}

public interface IBooleanResult : IResultBase<IBooleanResult>, IResultBase
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
}