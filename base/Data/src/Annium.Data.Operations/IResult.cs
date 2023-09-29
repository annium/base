namespace Annium.Data.Operations;

public interface IResult<TD> : IResultBase<IResult<TD>>, IDataResultBase<TD>
{
}

public interface IResult : IResultBase<IResult>, IResultBase
{
}